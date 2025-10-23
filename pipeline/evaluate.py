# -*- coding: utf-8 -*-
"""
Đánh giá ranker: Recall@K, nDCG@K trên logs/decisions.jsonl
(đơn giản: áp lại features & ranker -> so chosen có lọt @K không)
"""
import os, json, argparse, math
import numpy as np
import pandas as pd
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
            "faiss_sim": float(getattr(r,"faiss_sim", 0.0)),
            "cov_main": float(cvr),
            "risk": float(rsk),
            "cal_pen": float(cal_pen),
            "equip_ok": float(feas)
        })
    return pd.DataFrame(rows)

def recall_at_k(truth: set, ranked_ids: list, k=5) -> float:
    return 1.0 if len(truth & set(ranked_ids[:k]))>0 else 0.0

def ndcg_at_k(truth: set, ranked_ids: list, k=5) -> float:
    # binary relevance; 1 nếu id ∈ truth
    dcg = 0.0
    for i, rid in enumerate(ranked_ids[:k], start=1):
        rel = 1.0 if rid in truth else 0.0
        if rel>0:
            dcg += rel / np.log2(i+1)
    # ideal DCG: 1 ở vị trí 1
    idcg = 1.0  # vì chỉ cần một đúng là đủ
    return dcg/idcg

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes_with_text.csv")
    ap.add_argument("--logs_jsonl", default="logs/decisions.jsonl")
    ap.add_argument("--model", default="app/models/ranker.pkl")
    ap.add_argument("--k", type=int, default=5)
    args = ap.parse_args()

    model = joblib.load(args.model)

    df = pd.read_csv(args.recipes_csv)
    if "RecipeId" not in df.columns: df["RecipeId"] = df.index.astype(str)
    for col in ["Ingredients","Allergens","Equipment"]:
        if col in df.columns: df[col] = df[col].apply(parse_list)
        else: df[col] = [[] for _ in range(len(df))]
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"]*len(df)
    rec_map = df.set_index("RecipeId")

    recs, ndcgs, n = [], [], 0
    with open(args.logs_jsonl,"r",encoding="utf-8") as f:
        for line in f:
            obj = json.loads(line)
            for part in ["main","side"]:
                cand_ids = [c["RecipeId"] for c in obj["candidates"][part]]
                if not cand_ids: continue
                chosen = set(obj["chosen"][part])
                ctx = obj["context"]
                ctx["max_cal"] = ctx.get("max_cal_main" if part=="main" else "max_cal_side", 600)

                cand_df = rec_map.loc[cand_ids].reset_index()
                feat = build_features_for(cand_df, ctx)
                cols = model.feature_name_
                scores = model.predict(feat[cols])
                order = np.argsort(-scores)
                ranked = [cand_ids[i] for i in order]
                recs.append(recall_at_k(chosen, ranked, k=args.k))
                ndcgs.append(ndcg_at_k(chosen, ranked, k=args.k))
                n += 1

    print(f"[evaluate] sessions: {n}, Recall@{args.k}={np.mean(recs):.3f}, nDCG@{args.k}={np.mean(ndcgs):.3f}")

if __name__ == "__main__":
    main()
