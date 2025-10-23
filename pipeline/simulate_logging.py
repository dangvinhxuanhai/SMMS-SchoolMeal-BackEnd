# -*- coding: utf-8 -*-
"""
Sinh log giả để bootstrapping huấn luyện ranker.
Mỗi dòng logs/decisions.jsonl:
{
  "context": {...},
  "candidates": [{"RecipeId": "..."}],
  "chosen": {"main": [...], "side": [...]}
}
Bạn có thể thay bằng log thực tế của hệ thống sau này.
"""
import os, json, argparse, random, math
import numpy as np
import pandas as pd

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

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes_with_text.csv")
    ap.add_argument("--out_logs", default="logs/decisions.jsonl")
    ap.add_argument("--sessions", type=int, default=100)
    ap.add_argument("--seed", type=int, default=42)
    args = ap.parse_args()

    os.makedirs(os.path.dirname(args.out_logs), exist_ok=True)
    random.seed(args.seed); np.random.seed(args.seed)

    df = pd.read_csv(args.recipes_csv)
    if "RecipeId" not in df.columns: df["RecipeId"] = df.index.astype(str)
    for col in ["Ingredients","Allergens","Equipment"]:
        if col in df.columns: df[col] = df[col].apply(parse_list)
        else: df[col] = [[] for _ in range(len(df))]
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"]*len(df)

    mains = df[df["DishType"].str.lower().eq("main")]
    sides = df[df["DishType"].str.lower().eq("side")]

    with open(args.out_logs, "w", encoding="utf-8") as f:
        for s in range(args.sessions):
            # random context
            main_ings = random.sample(sum((parse_list(x) for x in df["Ingredients"]), []), k=min(3, len(df))) if len(df)>0 else []
            side_ings = random.sample(main_ings, k=min(2, len(main_ings))) if main_ings else []
            avoid_algs = random.sample(sum((parse_list(x) for x in df["Allergens"]), []), k=min(1, 3)) if random.random()<0.5 else []
            max_cal_main = random.choice([400,500,600,700])
            max_cal_side = random.choice([250,300,350,400])
            diners = random.choice([50, 80, 120, 200])

            ctx = {
                "main_ingredients": main_ings,
                "side_ingredients": side_ings,
                "avoid_allergens": avoid_algs,
                "max_cal_main": max_cal_main,
                "max_cal_side": max_cal_side,
                "diners_count": diners
            }

            # chọn ứng viên giả định (ở thực tế sẽ là FAISS top-k)
            cands_main = mains.sample(min(80, len(mains)), random_state=random.randint(0,1_000_000))
            cands_side = sides.sample(min(80, len(sides)), random_state=random.randint(0,1_000_000)) if len(sides)>0 else pd.DataFrame(columns=mains.columns)

            def cov(recipe_ings, target):
                a=set([i.lower() for i in recipe_ings]); b=set([i.lower() for i in target])
                return len(a&b)/(len(b) or 1)
            def risk(algs, avoid):
                a=set([i.lower() for i in algs]); b=set([i.lower() for i in avoid])
                return 1.0 if a & b else 0.0

            def score_rows(rows, target_ings, max_cal):
                sc=[]
                for r in rows.itertuples():
                    cvr = cov(getattr(r,"Ingredients",[]), target_ings)
                    cal = abs(float(getattr(r,"Calories",0.0)) - max_cal)/max(max_cal,1)
                    rsk = risk(getattr(r,"Allergens",[]), avoid_algs)
                    s = 1.2*cvr - 0.8*cal - 1.5*rsk + random.uniform(-0.05,0.05)
                    sc.append((r.RecipeId, s))
                sc.sort(key=lambda x:x[1], reverse=True)
                return [rid for rid,_ in sc]

            id_main_rank = score_rows(cands_main, main_ings, max_cal_main)
            id_side_rank = score_rows(cands_side, side_ings, max_cal_side) if len(cands_side)>0 else []

            chosen = {
                "main": id_main_rank[:5],
                "side": id_side_rank[:5]
            }

            # lưu log đơn giản
            line = {
                "context": ctx,
                "candidates": {"main": [ {"RecipeId": rid} for rid in id_main_rank[:50] ],
                               "side": [ {"RecipeId": rid} for rid in id_side_rank[:50] ]},
                "chosen": chosen
            }
            f.write(json.dumps(line, ensure_ascii=False) + "\n")

    print(f"[simulate_logging] Wrote {args.sessions} sessions -> {args.out_logs}")

if __name__ == "__main__":
    main()
