# app/services/features.py
# Chức năng:
# - Xây dựng vector feature cho từng ứng viên recipe dựa trên:
#     + Kết quả FAISS (faiss_sim)
#     + Graph (ingredient_coverage, allergen_risk, ppr_score)
#     + Ngữ cảnh (max_cal, avoid_allergens, available_equipment, group_rates)
# - Output dùng cho:
#     + MLRanker (online)
#     + score_linear (baseline)
#     + MMR (kết hợp với id2score / id2vec)

import numpy as np
import pandas as pd

def build_features(df_cands: pd.DataFrame, ctx, graph) -> pd.DataFrame:
    """
    Từ DataFrame ứng viên + context + graph,
    build ra DataFrame features với mỗi dòng là 1 recipe.

    Tham số:
        df_cands: DataFrame ứng viên, thường gồm:
            - RecipeId
            - Ingredients (list)
            - Allergens (list)
            - Equipment (list)
            - Calories
            - faiss_sim (nếu có)
        ctx: dict ngữ cảnh, ví dụ:
            - main_ings: list nguyên liệu chính từ request
            - avoid_allergens: list allergen cần tránh
            - available_equipment: list thiết bị khả dụng
            - group_rates: map allergen -> tỉ lệ dị ứng trong nhóm
            - max_cal: giới hạn calo cho nhóm (main/side)
        graph: instance KitchenGraph, cung cấp:
            - ingredient_coverage(...)
            - allergen_risk(...)
            - ppr_score(...)

    Trả về:
        DataFrame với các cột:
            RecipeId, faiss_sim, cov_main, risk, ppr, cal_pen, equip_ok
    """
    rows = []

    # Duyệt từng ứng viên trong df_cands
    for r in df_cands.itertuples():
        # Độ phủ nguyên liệu mong muốn (0..1)
        cov = graph.ingredient_coverage(
            getattr(r, "Ingredients", []),
            ctx["main_ings"],
        )

        # Điểm rủi ro dị ứng (kết hợp hard + group_rates)
        risk = graph.allergen_risk(
            getattr(r, "Allergens", []),
            ctx["avoid_allergens"],
            ctx.get("group_rates", {}),
        )

        # Personalized PageRank score trên graph:
        # Độ liên quan giữa main_ings và node ("Recipe", RecipeId)
        recipe_id = str(getattr(r, "RecipeId"))
        ppr = graph.ppr_score(
            ctx["main_ings"],
            ("Recipe", recipe_id),
        )

        # Penalty calo: độ lệch so với max_cal, chuẩn hoá để nằm [0, +∞)
        cal = float(getattr(r, "Calories", 0.0))
        max_cal = ctx["max_cal"]
        cal_pen = abs(cal - max_cal) / max(max_cal, 1)

        # Thiết bị: 1 nếu tất cả equipment của món nằm trong available_equipment, else 0
        equip = getattr(r, "Equipment", []) or []
        feas = (
            1.0
            if set(equip).issubset(set(ctx["available_equipment"] or []))
            else 0.0
        )

        # Ghi lại 1 dòng feature
        rows.append(
            {
                "RecipeId": getattr(r, "RecipeId"),
                # faiss_sim: nếu df_cands không có thì mặc định 0.0
                "faiss_sim": float(getattr(r, "faiss_sim", 0.0)),
                "cov_main": float(cov),
                "risk": float(risk),
                "ppr": float(ppr),
                "cal_pen": float(cal_pen),
                "equip_ok": float(feas),
            }
        )

    # Trả về DataFrame features cho toàn bộ candidates
    return pd.DataFrame(rows)
