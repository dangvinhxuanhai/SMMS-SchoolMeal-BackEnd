from sqlalchemy import text
import pandas as pd
from src.app.services.db import engine

def get_item_sets(food_ids):
    with engine.begin() as conn:
        q = """
        SELECT fi.food_id,
               ARRAY_AGG(DISTINCT i.name) FILTER (WHERE fii.is_main) AS main_ings,
               ARRAY_AGG(DISTINCT i.name) AS all_ings,
               ARRAY_AGG(DISTINCT a.name) AS allergens
        FROM food_items fi
        JOIN food_item_ingredients fii ON fi.food_id=fii.food_id
        JOIN ingredients i ON i.ingredient_id=fii.ingredient_id
        LEFT JOIN ingredients_allergens ia ON ia.ingredient_id=fii.ingredient_id
        LEFT JOIN allergens a ON a.allergen_id=ia.allergen_id
        WHERE fi.food_id = ANY(:ids)
        GROUP BY fi.food_id
        """
        df = pd.read_sql(text(q), conn, params={"ids": food_ids})
    # chuyển None -> empty list
    for col in ["main_ings","all_ings","allergens"]:
        df[col] = df[col].apply(lambda x: [] if x is None else list(dict.fromkeys(x)))
    return df

def jaccard(a, b):
    sa, sb = set(a), set(b)
    u = len(sa | sb)
    return 0.0 if u==0 else len(sa & sb) / u

def rank(candidates_df, sets_df, main_ingredients, need_ingredients, avoid_allergens):
    df = candidates_df.merge(sets_df, on="food_id", how="left")

    # hard filter
    def pass_filters(row):
        if set(avoid_allergens) & set(row["allergens"]): return False
        if need_ingredients and not set(need_ingredients).issubset(set(row["all_ings"])): return False
        return True
    df = df[df.apply(pass_filters, axis=1)]

    # features
    df["ingredient_overlap"] = df["main_ings"].apply(lambda ings: jaccard(ings, main_ingredients))
    df["pop_norm"] = (df["popularity"] - df["popularity"].min()) / max(1,(df["popularity"].max()-df["popularity"].min()))
    df["need_bonus"] = df["all_ings"].apply(lambda ings: 0.1 if set(need_ingredients).issubset(set(ings)) else 0.0)

    # final score
    df["score"] = (0.55*df["semantic_sim"]
                   + 0.25*df["ingredient_overlap"]
                   + 0.10*df["pop_norm"]
                   + 0.10*0.0   # placeholder nếu sau này có ML ranker
                   + df["need_bonus"])

    df = df.sort_values("score", ascending=False)
    return df
