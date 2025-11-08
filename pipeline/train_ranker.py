# -*- coding: utf-8 -*-
# train_ranker.py
# Mục đích:
# - Huấn luyện ranker (LightGBM LambdaMART) từ logs/decisions.jsonl.
# - Với mỗi phiên gợi ý (session) và mỗi nhóm (main/side), tạo:
#       + candidates: danh sách món được đề xuất cho model.
#       + features: đặc trưng dựa trên ngữ cảnh (allergens, calo, equipment, ...).
#       + label: 1 nếu RecipeId thuộc "chosen" (món được chọn thực tế), ngược lại 0.
# - Gom các phiên thành bài toán ranking (learning-to-rank) theo group (session).
# - Xuất model đã train: app/models/ranker.pkl để API online dùng trong bước rerank.

import os, json, argparse, math
import numpy as np
import pandas as pd
from lightgbm import LGBMRanker
import joblib


def parse_list(x):
    """
    Chuẩn hoá field list:
    - None / NaN           -> []
    - list                 -> strip từng phần tử
    - JSON list hợp lệ     -> parse thành list
    - "a,b,c"              -> tách theo dấu phẩy
    """
    if x is None or (isinstance(x, float) and math.isnan(x)):
        return []
    if isinstance(x, list):
        return [str(i).strip() for i in x if str(i).strip()]
    s = str(x).strip()
    if not s:
        return []
    try:
        j = json.loads(s)
        if isinstance(j, list):
            return [str(i).strip() for i in j if str(i).strip()]
    except Exception:
        pass
    return [p.strip() for p in s.split(",") if p.strip()]


def build_features_for(df_rows: pd.DataFrame, ctx: dict) -> pd.DataFrame:
    """
    Tạo bộ features cho các ứng viên trong MỘT context (main hoặc side) để train ranker.

    Features gồm:
    - faiss_sim : độ giống sematic (nếu có, offline có thể để 0)
    - cov_main  : độ phủ nguyên liệu mong muốn
    - risk      : món có dính allergen cần tránh hay không (0/1)
    - cal_pen   : độ lệch calo so với max_cal (chuẩn hoá)
    - equip_ok  : món có phù hợp thiết bị hiện có hay không (0/1)
    """

    def cov(recipe_ings, target):
        # Độ phủ nguyên liệu: tỉ lệ nguyên liệu target xuất hiện trong recipe
        a = set([i.lower() for i in (recipe_ings or [])])
        b = set([i.lower() for i in (target or [])])
        return len(a & b) / (len(b) or 1)

    def risk(algs, avoid):
        # Rủi ro dị ứng nhị phân: 1 nếu có ít nhất 1 allergen cần tránh, else 0
        a = set([i.lower() for i in (algs or [])])
        b = set([i.lower() for i in (avoid or [])])
        return 1.0 if a & b else 0.0

    rows = []
    max_cal = float(ctx.get("max_cal", 600))

    for r in df_rows.itertuples():
        # Độ phủ nguyên liệu chính
        cvr = cov(getattr(r, "Ingredients", []), ctx.get("main_ingredients", []))

        # Penalty calo: càng lệch max_cal càng lớn
        cal_pen = abs(float(getattr(r, "Calories", 0.0)) - max_cal) / max(max_cal, 1)

        # Rủi ro allergen
        rsk = risk(getattr(r, "Allergens", []), ctx.get("avoid_allergens", []))

        # Kiểm tra thiết bị: 1 nếu tập Equipment của món là tập con của available_equipment
        feas = (
            1.0
            if set(getattr(r, "Equipment", []) or []).issubset(
                set(ctx.get("available_equipment", []) or [])
            )
            else 0.0
        )

        rows.append(
            {
                "RecipeId": r.RecipeId,
                # faiss_sim: để đây cho đồng nhất schema với online; nếu offline không có thì là 0
                "faiss_sim": float(getattr(r, "faiss_sim", 0.0)),
                "cov_main": float(cvr),
                "risk": float(rsk),
                "cal_pen": float(cal_pen),
                "equip_ok": float(feas),
            }
        )

    return pd.DataFrame(rows)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--recipes_csv",
        default="data/recipes_with_text.csv",
        help="CSV đã chuẩn hoá recipes (Ingredients, Allergens, Equipment, ...)",
    )
    ap.add_argument(
        "--logs_jsonl",
        default="logs/decisions.jsonl",
        help="Log phiên gợi ý & lựa chọn thực tế (jsonl)",
    )
    ap.add_argument(
        "--out_model",
        default="app/models/ranker.pkl",
        help="Nơi lưu model ranker đã train",
    )
    args = ap.parse_args()

    # Đảm bảo thư mục output tồn tại
    os.makedirs(os.path.dirname(args.out_model), exist_ok=True)

    # --------- Load & chuẩn hoá recipes ---------
    df = pd.read_csv(args.recipes_csv)

    # Nếu thiếu RecipeId -> dùng index làm RecipeId
    if "RecipeId" not in df.columns:
        df["RecipeId"] = df.index.astype(str)

    # Chuẩn hoá list-like columns
    for col in ["Ingredients", "Allergens", "Equipment"]:
        if col in df.columns:
            df[col] = df[col].apply(parse_list)
        else:
            df[col] = [[] for _ in range(len(df))]

    # Nếu thiếu DishType, mặc định là "Main" (không ảnh hưởng nhiều ở đây)
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"] * len(df)

    # Map RecipeId -> row để join nhanh
    rec_map = df.set_index("RecipeId")

    # --------- Chuẩn bị dữ liệu train cho LightGBM Ranker ---------
    # X_list: list DataFrame features theo từng group
    # y_list: list Series labels tương ứng
    # qid_list: độ dài mỗi group (session/part) cho LGBMRanker.group
    X_list, y_list, qid_list = [], [], []

    # Mỗi dòng trong logs_jsonl là một session
    with open(args.logs_jsonl, "r", encoding="utf-8") as f:
        for sid, line in enumerate(f):
            obj = json.loads(line)
            ctx = obj["context"]

            # ====== Nhánh MAIN ======
            cand_ids = [c["RecipeId"] for c in obj["candidates"]["main"]]
            chosen = set(obj["chosen"]["main"])

            # Bỏ qua nếu không có ứng viên
            if cand_ids:
                cand_df = rec_map.loc[cand_ids].reset_index()
                # max_cal cho main lấy từ context
                ctx["max_cal"] = ctx.get("max_cal_main", 600)
                feat = build_features_for(cand_df, ctx)

                # Label: 1 nếu RecipeId ∈ chosen, else 0
                labels = [1 if rid in chosen else 0 for rid in feat["RecipeId"]]

                # Lưu group main
                X_list.append(feat.drop(columns=["RecipeId"]))
                y_list.append(pd.Series(labels))
                qid_list.append(len(labels))

            # ====== Nhánh SIDE (nếu có) ======
            if obj["candidates"]["side"]:
                cand_ids = [c["RecipeId"] for c in obj["candidates"]["side"]]
                chosen = set(obj["chosen"]["side"])

                if cand_ids:
                    cand_df = rec_map.loc[cand_ids].reset_index()
                    # Tạo context riêng cho side (copy để tránh ghi đè)
                    ctx2 = dict(ctx)
                    ctx2["max_cal"] = ctx.get("max_cal_side", 400)
                    feat2 = build_features_for(cand_df, ctx2)

                    labels2 = [1 if rid in chosen else 0 for rid in feat2["RecipeId"]]

                    X_list.append(feat2.drop(columns=["RecipeId"]))
                    y_list.append(pd.Series(labels2))
                    qid_list.append(len(labels2))

    # Ghép toàn bộ group lại thành một DataFrame/Series duy nhất
    X = pd.concat(X_list, axis=0).reset_index(drop=True)
    y = pd.concat(y_list, axis=0).reset_index(drop=True)

    # --------- Train LightGBM LambdaMART ---------
    print(f"[train_ranker] Train on {len(y)} samples, features={list(X.columns)}")

    model = LGBMRanker(
        objective="lambdarank",  # dùng LambdaMART cho ranking
        n_estimators=400,
        learning_rate=0.05,
        num_leaves=63,
        random_state=42,
    )

    # group=qid_list: độ dài từng query-group (mỗi main/side context là 1 group)
    model.fit(X, y, group=qid_list)

    # --------- Lưu model ---------
    joblib.dump(model, args.out_model)
    print(f"[train_ranker] Saved model -> {args.out_model}")


if __name__ == "__main__":
    # Chạy:
    #   python train_ranker.py --recipes_csv ... --logs_jsonl ... --out_model ...
    main()
