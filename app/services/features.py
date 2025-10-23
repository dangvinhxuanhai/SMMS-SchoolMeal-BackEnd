import numpy as np, pandas as pd

def build_features(df_cands: pd.DataFrame, ctx, graph) -> pd.DataFrame:
    rows = []
    for r in df_cands.itertuples():
        cov = graph.ingredient_coverage(r.Ingredients, ctx["main_ings"])
        risk = graph.allergen_risk(r.Allergens, ctx["avoid_allergens"], ctx.get("group_rates", {}))
        ppr = graph.ppr_score(ctx["main_ings"], ("Recipe", r.RecipeId))
        cal_pen = abs(float(r.Calories) - ctx["max_cal"]) / max(ctx["max_cal"], 1)
        feas = 1.0 if set(r.Equipment or []).issubset(set(ctx["available_equipment"] or [])) else 0.0
        rows.append({
            "RecipeId": r.RecipeId,
            "faiss_sim": getattr(r, "faiss_sim", 0.0),
            "cov_main": cov,
            "risk": risk,
            "ppr": ppr,
            "cal_pen": cal_pen,
            "equip_ok": feas,
        })
    return pd.DataFrame(rows)
