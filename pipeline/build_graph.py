# -*- coding: utf-8 -*-
"""
Xây NetworkX graph cho domain Kitchen:
Nodes: ("Recipe", id), ("Ingredient", name), ("Allergen", name), ("Equipment", name), ("Season", name)
Edges:
- Recipe --USES--> Ingredient
- Recipe --HAS_ALLERGEN--> Allergen
- Recipe --REQUIRES--> Equipment
- Recipe --SUITS_SEASON--> Season
"""

import os, argparse, json, math
import pandas as pd
import networkx as nx

# Hỗ trợ lưu gpickle tương thích cả NetworkX 2.x/3.x
try:
    from networkx.readwrite import gpickle as nx_gpickle  # NetworkX 3.x
except Exception:
    nx_gpickle = None
    import pickle

def save_gpickle(G, path):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    if nx_gpickle is not None:
        nx_gpickle.write_gpickle(G, path)
    else:
        with open(path, "wb") as f:
            pickle.dump(G, f)

def parse_list(x):
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

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes_with_text.csv")
    ap.add_argument("--out_graph",   default="graph/kitchen_graph.gpickle")
    args = ap.parse_args()

    os.makedirs(os.path.dirname(args.out_graph), exist_ok=True)

    df = pd.read_csv(args.recipes_csv)
    if "RecipeId" not in df.columns:
        df["RecipeId"] = df.index.astype(str)

    # Chuẩn hoá các cột list-like
    for col in ["Ingredients", "Allergens", "Equipment"]:
        if col in df.columns:
            df[col] = df[col].apply(parse_list)
        else:
            df[col] = [[] for _ in range(len(df))]

    season_col = "Season" if "Season" in df.columns else None

    G = nx.DiGraph()

    for r in df.itertuples():
        rid = str(getattr(r, "RecipeId"))
        rnode = ("Recipe", rid)
        name = str(getattr(r, "Name", rid))
        dish_type = str(getattr(r, "DishType", ""))

        G.add_node(rnode, Name=name, DishType=dish_type)

        # Ingredients
        for ing in getattr(r, "Ingredients", []):
            inode = ("Ingredient", ing.lower())
            G.add_node(inode)
            G.add_edge(rnode, inode, type="USES")

        # Allergens
        for alg in getattr(r, "Allergens", []):
            anode = ("Allergen", alg.lower())
            G.add_node(anode)
            G.add_edge(rnode, anode, type="HAS_ALLERGEN")

        # Equipment
        for eq in getattr(r, "Equipment", []):
            enode = ("Equipment", eq.lower())
            G.add_node(enode)
            G.add_edge(rnode, enode, type="REQUIRES")

        # Season
        if season_col:
            s = str(getattr(r, season_col)).strip()
            if s:
                snode = ("Season", s.lower())
                G.add_node(snode)
                G.add_edge(rnode, snode, type="SUITS_SEASON")

    save_gpickle(G, args.out_graph)
    print(f"[build_graph] Nodes: {G.number_of_nodes()}, Edges: {G.number_of_edges()}")
    print(f"[build_graph] Saved to: {args.out_graph}")

if __name__ == "__main__":
    main()
