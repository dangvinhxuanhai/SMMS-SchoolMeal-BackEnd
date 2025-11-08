# -*- coding: utf-8 -*-
# evaluate_ranker.py (tên gợi ý)
# Mục đích:
# - Đánh giá chất lượng model rerank (ranker.pkl) dựa trên log quyết định thực tế.
# - Input:
#     + recipes_with_text.csv: metadata + features cơ bản cho recipe.
#     + logs/decisions.jsonl: log các phiên gợi ý:
#           {
#             "context": {...},
#             "candidates": {
#                 "main": [ { "RecipeId": ... }, ... ],
#                 "side": [ ... ]
#             },
#             "chosen": {
#                 "main": [list RecipeId mà người dùng/chuyên gia đã chọn],
#                 "side": [...]
#             }
#           }
#     + model: file ranker.pkl đã train.
# - Cách làm:
#     1. Với mỗi phiên + mỗi nhóm (main/side):
#         - Lấy danh sách candidates.
#         - Tính features cho từng candidate.
#         - Cho model predict score, sort giảm dần.
#         - Đo xem các món "chosen" có xuất hiện trong top@K không.
#     2. Tính Recall@K và nDCG@K (binary relevance).
# - Output:
#     - In ra số phiên và trung bình Recall@K, nDCG@K để xem ranker hiệu quả tới đâu.

import os, json, argparse, math
import numpy as np
import pandas as pd
import joblib


def parse_list(x):
    """
    Chuẩn hoá field list tương tự các file khác:
    - None / NaN           -> []
    - list                 -> strip phần tử
    - JSON list hợp lệ     -> parse
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
    Build lại bộ features đơn giản cho các ứng viên trong 1 phiên log,
    dựa trên context (max_cal, main_ingredients, avoid_allergens, available_equipment, ...).

    Lưu ý:
    - Đây là phiên bản tối giản để phục vụ evaluate.
    - Feature names phải khớp với lúc train model (faiss_sim, cov_main, risk, cal_pen, equip_ok).
    """

    def cov(recipe_ings, target):
        # Độ phủ nguyên liệu: % target ingredient xuất hiện trong recipe
        a = set([i.lower() for i in (recipe_ings or [])])
        b = set([i.lower() for i in (target or [])])
        return len(a & b) / (len(b) or 1)

    def risk(algs, avoid):
        # Rủi ro dị ứng nhị phân: 1 nếu recipe có allergen cần tránh, ngược lại 0
        a = set([i.lower() for i in (algs or [])])
        b = set([i.lower() for i in (avoid or [])])
        return 1.0 if a & b else 0.0

    rows = []
    max_cal = float(ctx.get("max_cal", 600))

    for r in df_rows.itertuples():
        cvr = cov(getattr(r, "Ingredients", []), ctx.get("main_ingredients", []))

        # cal_pen: độ lệch calo so với max_cal, chuẩn hoá theo max_cal
        cal_pen = abs(float(getattr(r, "Calories", 0.0)) - max_cal) / max(max_cal, 1)

        rsk = risk(getattr(r, "Allergens", []), ctx.get("avoid_allergens", []))

        # equip_ok: 1 nếu mọi thiết bị cần đều có trong available_equipment, ngược lại 0
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
                "faiss_sim": float(getattr(r, "faiss_sim", 0.0)),  # similarity từ FAISS
                "cov_main": float(cvr),
                "risk": float(rsk),
                "cal_pen": float(cal_pen),
                "equip_ok": float(feas),
            }
        )

    return pd.DataFrame(rows)


def recall_at_k(truth: set, ranked_ids: list, k=5) -> float:
    """
    Recall@K dạng "hit-rate":
    - 1.0 nếu có ÍT NHẤT MỘT RecipeId trong truth xuất hiện trong top K.
    - 0.0 nếu không có cái nào.
    (Ở đây logs chosen có thể chứa 1+ món đúng.)
    """
    return 1.0 if len(truth & set(ranked_ids[:k])) > 0 else 0.0


def ndcg_at_k(truth: set, ranked_ids: list, k=5) -> float:
    """
    nDCG@K với binary relevance (1 nếu id ∈ truth, ngược lại 0).

    Cách đơn giản:
    - DCG = sum( rel_i / log2(i+1) ) với i là vị trí trong ranking (1-based).
    - IDCG: vì chỉ cần có 1 món đúng là đủ, ideal là món đúng ở vị trí 1 => IDCG = 1.
    => nDCG = DCG / 1 = DCG.

    Ý nghĩa:
    - 1.0: món đúng nằm top 1.
    - ~0.63: món đúng nằm vị trí 2.
    - Giảm dần nếu đúng ở vị trí sâu hơn.
    """
    dcg = 0.0
    for i, rid in enumerate(ranked_ids[:k], start=1):
        rel = 1.0 if rid in truth else 0.0
        if rel > 0:
            dcg += rel / np.log2(i + 1)

    idcg = 1.0  # với giả định chỉ cần 1 đúng là đủ (ideal ở rank 1)
    return dcg / idcg


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--recipes_csv",
        default="data/recipes_with_text.csv",
        help="CSV chứa recipes + faiss_sim + metadata",
    )
    ap.add_argument(
        "--logs_jsonl",
        default="logs/decisions.jsonl",
        help="File logs quyết định (jsonl) dùng để evaluate",
    )
    ap.add_argument(
        "--model",
        default="app/models/ranker.pkl",
        help="Đường dẫn model ranker.pkl",
    )
    ap.add_argument(
        "--k",
        type=int,
        default=5,
        help="Top K dùng cho Recall@K và nDCG@K",
    )
    args = ap.parse_args()

    # Load model ranker đã train
    model = joblib.load(args.model)

    # Load recipes
    df = pd.read_csv(args.recipes_csv)

    # Đảm bảo có RecipeId
    if "RecipeId" not in df.columns:
        df["RecipeId"] = df.index.astype(str)

    # Chuẩn hóa list-like cols
    for col in ["Ingredients", "Allergens", "Equipment"]:
        if col in df.columns:
            df[col] = df[col].apply(parse_list)
        else:
            df[col] = [[] for _ in range(len(df))]

    # Default DishType nếu thiếu (không critical cho evaluate simple này)
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"] * len(df)

    # Map RecipeId -> row để truy cập nhanh ứng viên
    rec_map = df.set_index("RecipeId")

    recs, ndcgs, n = [], [], 0  # lưu các giá trị Recall@K, nDCG@K cho từng case

    # Đọc logs theo từng dòng (một session/tình huống)
    with open(args.logs_jsonl, "r", encoding="utf-8") as f:
        for line in f:
            obj = json.loads(line)

            # Evaluate riêng cho 2 nhánh main / side
            for part in ["main", "side"]:
                cand_ids = [c["RecipeId"] for c in obj["candidates"][part]]
                if not cand_ids:
                    continue

                # Ground truth: set các RecipeId mà người dùng / chuyên gia đã chọn
                chosen = set(obj["chosen"][part])

                # Context: thông tin query, max_cal_main/side, allergen, equip,...
                ctx = obj["context"]
                # Map max_cal chung cho hàm build_features_for
                ctx["max_cal"] = ctx.get(
                    "max_cal_main" if part == "main" else "max_cal_side",
                    600,
                )

                # Lấy các dòng recipe tương ứng candidates
                cand_df = rec_map.loc[cand_ids].reset_index()

                # Build features (phải khớp với lúc train ranker)
                feat = build_features_for(cand_df, ctx)
                cols = model.feature_name_  # Đảm bảo đúng thứ tự feature như khi train

                # Predict score cho từng candidate
                scores = model.predict(feat[cols])

                # Lấy thứ tự sort giảm dần theo scores
                order = np.argsort(-scores)
                ranked = [cand_ids[i] for i in order]

                # Tính metric cho case này
                recs.append(recall_at_k(chosen, ranked, k=args.k))
                ndcgs.append(ndcg_at_k(chosen, ranked, k=args.k))
                n += 1

    # In kết quả trung bình
    print(
        f"[evaluate] sessions: {n}, "
        f"Recall@{args.k}={np.mean(recs):.3f}, "
        f"nDCG@{args.k}={np.mean(ndcgs):.3f}"
    )


if __name__ == "__main__":
    # Chạy:
    #   python evaluate_ranker.py --model app/models/ranker.pkl --k 5
    main()
