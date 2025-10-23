import pandas as pd

def hard_filter(df: pd.DataFrame, ctx) -> pd.DataFrame:
    f = df.copy()
    # loại allergen cứng
    if ctx["avoid_allergens"]:
        avoid = {a.lower() for a in ctx["avoid_allergens"]}
        def ok(allergens): 
            s = {str(x).lower() for x in (allergens or [])}
            return len(s & avoid) == 0
        f = f[f["Allergens"].apply(ok)]
    # calories
    f = f[f["Calories"] <= ctx["max_cal"]]
    # equipment
    if ctx.get("available_equipment"):
        have = set(ctx["available_equipment"])
        def eq_ok(eq): return set(eq or []).issubset(have)
        f = f[f["Equipment"].apply(eq_ok)]
    return f
def ingredient_match(recipe_ings, targets, require_all=False, ratio_thresh=0.5):
    r = {str(x).strip().lower() for x in (recipe_ings or [])}
    t = {str(x).strip().lower() for x in (targets or [])}
    if not t:
        return True
    inter = len(r & t)
    if require_all:
        return inter == len(t)          # AND
    # ANY/ratio:
    return inter >= 1 or inter / (len(t) or 1) >= ratio_thresh
