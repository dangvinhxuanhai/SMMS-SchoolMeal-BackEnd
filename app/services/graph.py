# app/services/graph.py
import os
import math
import importlib
import networkx as nx

def _read_gpickle(path: str):
    """
    Đọc graph từ .gpickle, tương thích NetworkX 2.x/3.x.
    Ưu tiên networkx.readwrite.gpickle, nếu không có thì dùng pickle.
    """
    try:
        nx_gpickle = importlib.import_module("networkx.readwrite.gpickle")
        return nx_gpickle.read_gpickle(path)
    except Exception:
        import pickle
        with open(path, "rb") as f:
            return pickle.load(f)

class KitchenGraph:
    def __init__(self, path_gpickle: str):
        if not os.path.exists(path_gpickle):
            raise FileNotFoundError(f"Graph file not found: {path_gpickle}")
        self.G = _read_gpickle(path_gpickle)

    # ---------- Các hàm chấm điểm/feature trên graph ----------

    @staticmethod
    def ingredient_coverage(recipe_ings, target_ings) -> float:
        s = {str(x).lower() for x in (recipe_ings or [])}
        t = {str(x).lower() for x in (target_ings or [])}
        return len(s & t) / (len(t) or 1)

    @staticmethod
    def allergen_risk(recipe_allergens, avoid_allergens, group_rates: dict[str, float] = None) -> float:
        group_rates = group_rates or {}
        a = {str(x).lower() for x in (recipe_allergens or [])}
        b = {str(x).lower() for x in (avoid_allergens or [])}
        hard = 1.0 if (a & b) else 0.0
        soft = sum(float(group_rates.get(x, 0.0)) for x in a)
        return 10.0 * hard + soft  # hard >> soft

    def ppr_score(self, seed_ings, recipe_node: tuple, alpha: float = 0.85) -> float:
        """
        Personalized PageRank từ seed ingredients tới node món ăn.
        recipe_node gợi ý dạng: ("Recipe", recipe_id)
        """
        seeds = {str(n): 1.0 for n in (seed_ings or []) if (str(n) in self.G)}
        if not seeds:
            return 0.0
        pr = nx.pagerank(self.G, alpha=alpha, personalization=seeds, max_iter=50)
        return float(pr.get(recipe_node, 0.0))
