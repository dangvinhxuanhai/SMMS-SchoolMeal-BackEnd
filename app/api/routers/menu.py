# app/api/routers/menu.py
# -*- coding: utf-8 -*-
from fastapi import APIRouter
from app.core.config import *
from app.models.schemas import RecommendRequest, RecommendResponse
from app.services.embedder import Embedder
from app.services.retrieval import FaissRetriever
from app.services.graph import KitchenGraph
from app.services.filters import hard_filter
from app.services.features import build_features
from app.services.rerank import MLRanker, mmr_select

import pandas as pd
import numpy as np
from pathlib import Path

router = APIRouter(prefix="/menu", tags=["menu"])

# ----------------------------
# NẠP TÀI NGUYÊN 1 LẦN
# ----------------------------
DF = pd.read_csv(DATA_CSV)                 # CSV đã chuẩn hoá cột
EMB = Embedder(EMBED_MODEL)                # SentenceTransformer / OpenAI Embeddings
RET = FaissRetriever(INDEX_PATH, DF)       # FAISS + metadata (có thể có emb_pos)
KG  = KitchenGraph(GRAPH_PATH)             # Graph gpickle
RANKER = MLRanker(RANKER_PKL if USE_ML else None)

# Nạp ma trận embedding để phục vụ MMR (cosine đa dạng hoá)
INDEX_DIR = Path(INDEX_PATH).parent
EMB_PATH = str(INDEX_DIR / "recipe_embeddings.npy")
try:
    EMB_MATRIX = np.load(EMB_PATH)
except Exception:
    EMB_MATRIX = None  # vẫn chạy được nhưng MMR sẽ fallback theo score


# ----------------------------
# TIỆN ÍCH: CHUẨN HOÁ INPUT CHO MMR (AN TOÀN, KHỬ TRÙNG LẶP)
# ----------------------------
def _mmr_inputs(dfpart: pd.DataFrame, scores: pd.Series):
    """
    Trả về (ids_sorted, id2vec, id2score) với ID nhất quán và KHÔNG trùng lặp.
    - Ưu tiên 'emb_pos' nếu có; fallback dùng index dfpart (giả định = vị trí embedding).
    - Map điểm theo vị trí (zip) để tránh .loc trả Series khi index trùng.
    """
    if dfpart.empty:
        return [], {}, {}

    df = dfpart.copy()

    # 1) Chuẩn hoá index về int (nếu được)
    try:
        df.index = df.index.astype(int)
    except Exception:
        df.index = [int(i) if str(i).isdigit() else i for i in df.index]

    # 2) Đảm bảo 'scores' là Series cùng chiều & index với df
    if not isinstance(scores, pd.Series):
        scores = pd.Series(scores, index=df.index)
    else:
        if len(scores) != len(df):
            # không cùng chiều -> lỗi rõ ràng
            raise RuntimeError("scores length != dfpart length")
        # align theo vị trí (positional), không dựa vào index cũ
        scores = pd.Series(scores.values, index=df.index)

    # 3) Khử trùng lặp index (giữ dòng đầu tiên)
    mask_unique = ~pd.Index(df.index).duplicated(keep="first")
    if not mask_unique.all():
        df = df[mask_unique]
        scores = scores[mask_unique]

    # 4) id2score: map theo vị trí (zip), tránh .loc
    id2score = {int(i): float(s) for i, s in zip(df.index, scores.values)}

    # 5) id2vec: ưu tiên 'emb_pos' nếu có, fallback dùng index
    id2vec = {}
    if EMB_MATRIX is not None:
        if "emb_pos" in df.columns:
            emb_pos = df["emb_pos"].astype(int)
            for i, pos in zip(df.index, emb_pos):
                if 0 <= int(pos) < EMB_MATRIX.shape[0]:
                    id2vec[int(i)] = EMB_MATRIX[int(pos)]
        else:
            for i in df.index:
                if isinstance(i, int) and 0 <= i < EMB_MATRIX.shape[0]:
                    id2vec[int(i)] = EMB_MATRIX[int(i)]

    # 6) ids_sorted theo điểm giảm dần
    ids_sorted = sorted(id2score.keys(), key=lambda k: id2score[k], reverse=True)

    # 7) Lọc giữ id có đủ vec + score (tránh KeyError)
    ids_sorted = [i for i in ids_sorted if i in id2score and i in id2vec]

    return ids_sorted, id2vec, id2score


# ----------------------------
# BLOCK RERANK + MMR CHO 1 NHÓM (MAIN/SIDE)
# ----------------------------
def _do_block(dfpart: pd.DataFrame, max_n: int, ctx) -> list:
    if dfpart.empty:
        return []

    # build feature cho reranker, cố gắng giữ index khớp dfpart
    feat = build_features(dfpart, ctx, KG)
    if not feat.index.equals(dfpart.index):
        try:
            feat = feat.set_index(dfpart.index)
        except Exception:
            pass

    scores_arr = RANKER.predict(feat)  # ndarray; nếu USE_ML=False, MLRanker sẽ trả score mặc định (vd faiss_sim)
    scores = pd.Series(scores_arr, index=dfpart.index)

    # Chuẩn bị inputs cho MMR theo ID nhất quán
    try:
        ids_sorted, id2vec, id2score = _mmr_inputs(dfpart, scores)
    except Exception:
        # nếu build inputs fail, fallback lấy top-N theo score
        picked = (dfpart.assign(_score=scores)
                        .sort_values("_score", ascending=False)
                        .head(max_n)[["RecipeId", "Name", "Calories", "Allergens"]])
        return picked.to_dict(orient="records")

    # Nếu thiếu vector (không chạy được MMR) -> fallback theo score
    if not ids_sorted or not id2vec:
        picked = (dfpart.assign(_score=scores)
                        .sort_values("_score", ascending=False)
                        .head(max_n)[["RecipeId", "Name", "Calories", "Allergens"]])
        return picked.to_dict(orient="records")

    # Chạy MMR an toàn
    try:
        chosen_keys = mmr_select(ids_sorted, id2vec, id2score, top_n=max_n, lam=0.7)
    except Exception:
        chosen_keys = ids_sorted[:max_n]

    # Chọn lại hàng theo index và sort theo score giảm dần
    df_sel = dfpart.loc[dfpart.index.intersection(chosen_keys)].copy()
    df_sel["_score"] = [id2score.get(int(i), 0.0) for i in df_sel.index]
    df_sel = df_sel.sort_values("_score", ascending=False)

    return df_sel[["RecipeId", "Name", "Calories", "Allergens"]].to_dict(orient="records")


# ----------------------------
# ENDPOINT
# ----------------------------
@router.post("/recommend", response_model=RecommendResponse)
def recommend(req: RecommendRequest):
    # 1) Query -> embed (ghép nguyên liệu nếu không có query)
    qtxt = " ".join((req.main_ingredients or []) + (req.side_ingredients or [])) or (req.query or "")
    qv = EMB.encode([qtxt])  # (1, dim), đã normalize

    # 2) Retriever
    cands = RET.search(qv, k=K_SEARCH)  # DataFrame: thường gồm RecipeId, DishType, Allergens, Calories, faiss_sim, emb_pos,...

    # 3) Filter cứng cho Main/Side
    base_ctx = dict(
        main_ings=req.main_ingredients or [],
        avoid_allergens=req.avoid_allergens or [],
        available_equipment=req.available_equipment or [],
        group_rates=req.group_allergy_rates or {},
    )

    mains = cands[cands["DishType"].eq("Main")].copy()
    ctx_main = dict(base_ctx, max_cal=req.max_cal_main)
    mains = hard_filter(mains, ctx_main)

    sides = cands[cands["DishType"].eq("Side")].copy()
    ctx_side = dict(base_ctx, max_cal=req.max_cal_side)
    sides = hard_filter(sides, ctx_side)

    if mains.empty and sides.empty:
        return {"main": [], "side": [], "why": "Không có món phù hợp theo tiêu chí."}

    # 4) Features + ML score + MMR (hai nhánh)
    out = {"main": [], "side": [], "why": ""}

    out["main"] = _do_block(mains, req.top_n_main, ctx_main)
    out["side"] = _do_block(sides, req.top_n_side, ctx_side)

    # 5) Why (tuỳ chọn): có thể gọi LLM để giải thích
    out["why"] = "Gợi ý dựa trên FAISS (ngữ nghĩa), lọc ràng buộc (dị ứng/calo/thiết bị), rerank (ML) + đa dạng hoá (MMR)."

    return out
