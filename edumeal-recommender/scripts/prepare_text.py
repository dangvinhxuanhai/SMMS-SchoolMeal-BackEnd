# scripts/prepare_text.py
import os
from pathlib import Path
import pandas as pd
from sqlalchemy import create_engine, text
from dotenv import load_dotenv

load_dotenv()  # load .env ở project root

DATABASE_URL = os.getenv("DATABASE_URL")
if not DATABASE_URL:
    raise ValueError("DATABASE_URL is missing in .env")

engine = create_engine(DATABASE_URL, pool_pre_ping=True)
Path("data").mkdir(parents=True, exist_ok=True)

# Lấy text cho embedding (PostgreSQL)
SQL = """
SELECT fi.food_id,
       fi.name,
       COALESCE(fi.description,'') AS description,
       STRING_AGG(DISTINCT i.name, ', ') FILTER (WHERE fii.is_main) AS main_ings,
       STRING_AGG(DISTINCT i2.name, ', ') AS all_ings,
       STRING_AGG(DISTINCT a.name, ', ') AS allergens
FROM food_items fi
JOIN food_item_ingredients fii ON fi.food_id=fii.food_id
JOIN ingredients i ON i.ingredient_id=fii.ingredient_id
LEFT JOIN ingredients i2 ON i2.ingredient_id=fii.ingredient_id
LEFT JOIN ingredients_allergens ia ON ia.ingredient_id=fii.ingredient_id
LEFT JOIN allergens a ON a.allergen_id=ia.allergen_id
GROUP BY fi.food_id, fi.name, fi.description
ORDER BY fi.food_id;
"""

with engine.begin() as conn:
    df = pd.read_sql(text(SQL), conn)

df["doc_text"] = (
    "Name: " + df["name"].fillna("") + ". "
    + "Description: " + df["description"].fillna("") + ". "
    + "Main ingredients: " + df["main_ings"].fillna("") + ". "
    + "All ingredients: " + df["all_ings"].fillna("") + ". "
    + "Allergens: " + df["allergens"].fillna("") + "."
)

out = "data/recipes_text.csv"
df.to_csv(out, index=False, encoding="utf-8")
print(f"✅ Saved {out} with {len(df)} rows")
