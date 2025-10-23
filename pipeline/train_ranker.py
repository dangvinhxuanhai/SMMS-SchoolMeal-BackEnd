# -*- coding: utf-8 -*-
"""
Train ranker (LightGBM LambdaMART) từ logs/decisions.jsonl
- Tính feature cho mỗi candidate theo context (cùng công thức offline)
- Label: 1 nếu RecipeId thuộc chosen, else 0 (pointwise) — đơn giản & hiệu quả ban đầu
Xuất: app/models/ranker.pkl (để online dùng)
"""
import os, json, argparse, math
import numpy as np
import pandas as pd
from lightgbm import LGBMRanker
import joblib

def parse_list(x):
    if x is None or (isinstance(x, float) and math.isnan(x)): return []
    if isinstance(x, list): return [str(i).strip() for i in x if str(i).strip()]
    s = str(x).strip()
    if not s: return []
    try:
        j = json.loads(s)
        if isinstance(j, list):
            return [str(i).strip() for i in j if str(i).strip()]
    except Exception:
        pass
    return [p.strip() for p in s.split(",") if p.strip()]

def build_features_for(df_rows: pd.DataFrame, ctx: dict) -> pd.DataFrame:
    def cov(recipe_ings, target):
        a=set([i.lower() for i in recipe_ings or []]); b=set([i.lower() for i in target or []])
        return len(a&b)/(len(b) or 1)
    def risk(algs, avoid):
        a=set([i.lower() for i in algs or []]); b=set([i.lower() for i in avoid or []])
        return 1.0 if a & b else 0.0

    rows=[]
    max_cal = float(ctx.get("max_cal", 600))
    for r in df_rows.itertuples():
        cvr = cov(getattr(r,"Ingredients",[]), ctx.get("main_ingredients",[]))
        cal_pen = abs(float(getattr(r,"Calories",0.0)) - max_cal)/max(max_cal,1)
        rsk = risk(getattr(r,"Allergens",[]), ctx.get("avoid_allergens",[]))
        feas = 1.0 if set(getattr(r,"Equipment",[]) or []).issubset(set(ctx.get("available_equipment",[]) or [])) else 0.0
        rows.append({
            "RecipeId": r.RecipeId,
            "faiss_sim": float(getattr(r,"faiss_sim", 0.0)),   # nếu có (offline có thể set 0)
            "cov_main": float(cvr),
            "risk": float(rsk),
            "cal_pen": float(cal_pen),
            "equip_ok": float(feas)
        })
    return pd.DataFrame(rows)

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes_with_text.csv")
    ap.add_argument("--logs_jsonl", default="logs/decisions.jsonl")
    ap.add_argument("--out_model", default="app/models/ranker.pkl")
    args = ap.parse_args()

    os.makedirs(os.path.dirname(args.out_model), exist_ok=True)

    df = pd.read_csv(args.recipes_csv)
    if "RecipeId" not in df.columns: df["RecipeId"] = df.index.astype(str)
    for col in ["Ingredients","Allergens","Equipment"]:
        if col in df.columns: df[col] = df[col].apply(parse_list)
        else: df[col] = [[] for _ in range(len(df))]
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"]*len(df)
    # index theo RecipeId để join nhanh
    rec_map = df.set_index("RecipeId")

    X_list, y_list, qid_list = [], [], []   # LightGBM group theo phiên (session)
    with open(args.logs_jsonl,"r",encoding="utf-8") as f:
        for sid, line in enumerate(f):
            obj = json.loads(line)
            ctx = obj["context"]
            # train main
            cand_ids = [c["RecipeId"] for c in obj["candidates"]["main"]]
            chosen = set(obj["chosen"]["main"])
            cand_df = rec_map.loc[cand_ids].reset_index()
            ctx["max_cal"] = ctx.get("max_cal_main", 600)
            feat = build_features_for(cand_df, ctx)
            labels = [1 if rid in chosen else 0 for rid in feat["RecipeId"]]
            X_list.append(feat.drop(columns=["RecipeId"]))
            y_list.append(pd.Series(labels))
            qid_list.append(len(labels))

            # train side (nếu có)
            if obj["candidates"]["side"]:
                cand_ids = [c["RecipeId"] for c in obj["candidates"]["side"]]
                chosen = set(obj["chosen"]["side"])
                cand_df = rec_map.loc[cand_ids].reset_index()
                ctx2 = dict(ctx); ctx2["max_cal"] = ctx.get("max_cal_side", 400)
                feat2 = build_features_for(cand_df, ctx2)
                labels2 = [1 if rid in chosen else 0 for rid in feat2["RecipeId"]]
                X_list.append(feat2.drop(columns=["RecipeId"]))
                y_list.append(pd.Series(labels2))
                qid_list.append(len(labels2))

    X = pd.concat(X_list, axis=0).reset_index(drop=True)
    y = pd.concat(y_list, axis=0).reset_index(drop=True)

    # LightGBM Ranker
    print(f"[train_ranker] Train on {len(y)} samples, features={list(X.columns)}")
    model = LGBMRanker(
        objective="lambdarank",
        n_estimators=400,
        learning_rate=0.05,
        num_leaves=63,
        random_state=42
    )
    model.fit(X, y, group=qid_list)

    joblib.dump(model, args.out_model)
    print(f"[train_ranker] Saved model -> {args.out_model}")

if __name__ == "__main__":
    main()
