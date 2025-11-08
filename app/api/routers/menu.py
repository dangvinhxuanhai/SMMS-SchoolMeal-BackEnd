# app/api/routers/menu.py           # Định nghĩa router API cho chức năng gợi ý menu
# -*- coding: utf-8 -*-             # Khai báo encoding UTF-8 để hỗ trợ tiếng Việt trong file

from fastapi import APIRouter       # APIRouter dùng để nhóm các endpoint liên quan đến /menu
from app.core.config import *       # Import toàn bộ config (đường dẫn CSV, INDEX_PATH, EMBED_MODEL, K_SEARCH, USE_ML, ...)
from app.models.schemas import RecommendRequest, RecommendResponse  # Schema request/response cho endpoint recommend
from app.services.embedder import Embedder          # Service tạo embedding từ text truy vấn/nguyên liệu
from app.services.retrieval import FaissRetriever   # Service tìm kiếm ứng viên công thức bằng FAISS
from app.services.graph import KitchenGraph         # Service load đồ thị bếp (quan hệ món-nguyên liệu-allergen-...)
from app.services.filters import hard_filter        # Hàm lọc cứng theo ràng buộc (allergen, calo, thiết bị, ...)
from app.services.features import build_features    # Hàm xây feature cho model rerank ML
from app.services.rerank import MLRanker, mmr_select  # MLRanker: rerank bằng ML; mmr_select: chọn đa dạng theo MMR

import pandas as pd                 # Thư viện xử lý bảng dữ liệu
import numpy as np                  # Thư viện xử lý số, mảng, vector
from pathlib import Path            # Hỗ trợ xử lý đường dẫn file độc lập hệ điều hành

# Khởi tạo router với prefix URL /menu và nhóm tài liệu OpenAPI dưới tag "menu"
router = APIRouter(prefix="/menu", tags=["menu"])

# ----------------------------
# NẠP TÀI NGUYÊN 1 LẦN (tải sẵn vào RAM khi app khởi động)
# ----------------------------
DF = pd.read_csv(DATA_CSV)                 # Đọc file CSV chứa metadata các recipe (đã chuẩn hoá cột)
EMB = Embedder(EMBED_MODEL)                # Tạo đối tượng Embedder với model cấu hình (SentenceTransformer/OpenAI)
RET = FaissRetriever(INDEX_PATH, DF)       # Tạo retriever FAISS dùng index + DF để map id -> recipe info
KG  = KitchenGraph(GRAPH_PATH)             # Load đồ thị kiến thức (graph) từ file gpickle
RANKER = MLRanker(RANKER_PKL if USE_ML else None)  # Tạo MLRanker, nếu USE_ML=False thì bên trong sẽ fallback heuristic

# Xác định đường dẫn thư mục chứa file index FAISS
INDEX_DIR = Path(INDEX_PATH).parent        # Lấy folder cha của file index
EMB_PATH = str(INDEX_DIR / "recipe_embeddings.npy")  # Đường dẫn file numpy lưu toàn bộ embedding recipe

# Thử nạp ma trận embedding để phục vụ MMR; nếu lỗi thì cho phép chạy không MMR (fallback)
try:
    EMB_MATRIX = np.load(EMB_PATH)         # Ma trận (num_recipes, dim) dùng để tính độ đa dạng
except Exception:
    EMB_MATRIX = None                      # Nếu không có thì để None, MMR sẽ không dùng được và fallback theo score


# ----------------------------
# TIỆN ÍCH: CHUẨN HOÁ INPUT CHO MMR (AN TOÀN, KHỬ TRÙNG LẶP)
# ----------------------------
def _mmr_inputs(dfpart: pd.DataFrame, scores: pd.Series):
    """
    Chuẩn hóa dữ liệu đầu vào cho MMR.
    Trả về (ids_sorted, id2vec, id2score) với:
    - ids_sorted: danh sách id (index) đã sắp xếp theo score giảm dần, không trùng.
    - id2vec: map id -> vector embedding tương ứng.
    - id2score: map id -> score (đã align đúng theo vị trí).
    Ưu tiên dùng cột 'emb_pos' nếu có; nếu không, dùng luôn index DataFrame.
    Mục tiêu: tránh bug do index trùng hoặc lệch với ma trận embedding.
    """
    if dfpart.empty:                        # Nếu không có ứng viên thì trả về rỗng
        return [], {}, {}

    df = dfpart.copy()                      # Làm bản copy để không đụng vào df gốc

    # 1) Cố gắng ép index về int (nếu được)
    try:
        df.index = df.index.astype(int)     # Nếu index toàn số thì chuyển sang int luôn
    except Exception:
        # Nếu có phần tử không ép được trực tiếp, thử xử lý từng cái
        df.index = [int(i) if str(i).isdigit() else i for i in df.index]

    # 2) Đảm bảo 'scores' là Series cùng chiều & align theo vị trí với df
    if not isinstance(scores, pd.Series):   # Nếu scores là list/ndarray -> convert sang Series
        scores = pd.Series(scores, index=df.index)
    else:
        if len(scores) != len(df):          # Nếu độ dài không khớp -> lỗi rõ ràng (logic sai)
            raise RuntimeError("scores length != dfpart length")
        # Ép align theo thứ tự (positional), bỏ qua index cũ của scores
        scores = pd.Series(scores.values, index=df.index)

    # 3) Khử trùng lặp index (nếu có), giữ lại occurrence đầu tiên
    mask_unique = ~pd.Index(df.index).duplicated(keep="first")
    if not mask_unique.all():
        df = df[mask_unique]
        scores = scores[mask_unique]

    # 4) Tạo map id2score: dùng zip(index, score) tránh .loc gây Series khi trùng index
    id2score = {int(i): float(s) for i, s in zip(df.index, scores.values)}

    # 5) Tạo map id2vec: ưu tiên dùng 'emb_pos' nếu có, nếu không dùng index làm vị trí trong EMB_MATRIX
    id2vec = {}
    if EMB_MATRIX is not None:              # Chỉ làm nếu đã load được ma trận embedding
        if "emb_pos" in df.columns:         # Nếu có cột emb_pos (chỉ vị trí của embedding cho recipe này)
            emb_pos = df["emb_pos"].astype(int)
            for i, pos in zip(df.index, emb_pos):
                if 0 <= int(pos) < EMB_MATRIX.shape[0]:
                    id2vec[int(i)] = EMB_MATRIX[int(pos)]
        else:                               # Không có emb_pos -> giả định index trùng với dòng trong EMB_MATRIX
            for i in df.index:
                if isinstance(i, int) and 0 <= i < EMB_MATRIX.shape[0]:
                    id2vec[int(i)] = EMB_MATRIX[int(i)]

    # 6) Sắp xếp id theo score giảm dần
    ids_sorted = sorted(id2score.keys(), key=lambda k: id2score[k], reverse=True)

    # 7) Giữ lại những id có cả score và vector (tránh KeyError khi chạy MMR)
    ids_sorted = [i for i in ids_sorted if i in id2score and i in id2vec]

    return ids_sorted, id2vec, id2score


# ----------------------------
# BLOCK RERANK + MMR CHO 1 NHÓM (MAIN hoặc SIDE)
# ----------------------------
def _do_block(dfpart: pd.DataFrame, max_n: int, ctx) -> list:
    """
    Xử lý một nhóm ứng viên (main/side):
    - Khử trùng lặp index
    - Build feature
    - Rerank bằng MLRanker
    - Áp dụng MMR (nếu có embedding) để đa dạng hóa
    - Trả về list dict các món đã chọn (RecipeId, Name, Calories, Allergens)
    """
    if dfpart.empty:                        # Không có ứng viên -> trả về list rỗng
        return []

    # CHẮN CỔNG 1: Khử trùng lặp theo index (nếu cùng index thì chỉ giữ dòng đầu)
    dfpart = dfpart[~pd.Index(dfpart.index).duplicated(keep="first")].copy()

    # Build feature cho reranker; cố gắng giữ index giống dfpart để map score chính xác
    feat = build_features(dfpart, ctx, KG)  # Tạo DataFrame features từ ứng viên + context + graph
    if not feat.index.equals(dfpart.index): # Nếu index không khớp (do hàm build_features thay đổi)
        try:
            feat = feat.set_index(dfpart.index)  # Cố gắng ép index lại cho trùng
        except Exception:
            pass                                # Nếu không được thì thôi, vẫn dùng như cũ (ít nhất cùng chiều)

    scores_arr = RANKER.predict(feat)       # Gọi model ML để dự đoán score (nếu không có model -> dùng heuristic)
    scores = pd.Series(scores_arr, index=dfpart.index)  # Đưa về Series gắn index ứng viên

    # Chuẩn bị input cho MMR với id/score/vec nhất quán
    try:
        ids_sorted, id2vec, id2score = _mmr_inputs(dfpart, scores)
    except Exception:
        # Nếu _mmr_inputs lỗi (data bẩn, lệch kích thước,...) -> fallback: sort theo score và lấy top N
        picked = (dfpart.assign(_score=scores)
                        .sort_values("_score", ascending=False))
        # CHẮN CỔNG 2: Khử trùng lặp theo RecipeId (trùng công thức) rồi cắt top N
        picked = (picked[["RecipeId", "Name", "Calories", "Allergens"]]
                        .drop_duplicates(subset=["RecipeId"], keep="first")
                        .head(max_n))
        return picked.to_dict(orient="records")  # Trả về list các dict

    # Nếu không có đủ vector để chạy MMR -> fallback theo score
    if not ids_sorted or not id2vec:
        picked = (dfpart.assign(_score=scores)
                        .sort_values("_score", ascending=False))
        picked = (picked[["RecipeId", "Name", "Calories", "Allergens"]]
                        .drop_duplicates(subset=["RecipeId"], keep="first")
                        .head(max_n))
        return picked.to_dict(orient="records")

    # Thử chạy MMR để chọn top_n vừa tốt (score cao) vừa đa dạng
    try:
        chosen_keys = mmr_select(
            ids_sorted, id2vec, id2score,
            top_n=max_n,
            lam=0.7                      # lam (lambda): trade-off giữa relevance và diversity
        )
    except Exception:
        # Nếu MMR lỗi -> fallback chỉ lấy theo thứ tự score
        chosen_keys = ids_sorted[:max_n]

    # Lấy lại các dòng có index thuộc chosen_keys
    df_sel = dfpart.loc[dfpart.index.intersection(chosen_keys)].copy()
    # Gắn lại _score từ id2score để sort theo điểm giảm dần
    df_sel["_score"] = [id2score.get(int(i), 0.0) for i in df_sel.index]
    df_sel = df_sel.sort_values("_score", ascending=False)

    # CHẮN CỔNG 2: Đảm bảo không trùng RecipeId + cắt còn max_n món
    df_sel = (df_sel[["RecipeId", "Name", "Calories", "Allergens"]]
                    .drop_duplicates(subset=["RecipeId"], keep="first")
                    .head(max_n))

    return df_sel.to_dict(orient="records")  # Trả về list recipe đã chọn cho nhóm này



# ----------------------------
# ENDPOINT GỢI Ý MENU
# ----------------------------
@router.post("/recommend", response_model=RecommendResponse)  # Định nghĩa POST /menu/recommend, validate bằng RecommendResponse
def recommend(req: RecommendRequest):
    # 1) Tạo câu truy vấn văn bản:
    #    - Nếu có danh sách nguyên liệu main/side thì ghép lại thành text
    #    - Nếu không có thì dùng req.query
    qtxt = " ".join((req.main_ingredients or []) + (req.side_ingredients or [])) or (req.query or "")
    qv = EMB.encode([qtxt])  # Sinh embedding cho câu truy vấn (shape (1, dim)), thường đã được normalize

    # 2) Dùng FAISS retriever lấy k ứng viên gần nhất (semantic search)
    #    cands là DataFrame chứa các món ứng viên (RecipeId, DishType, Allergens, Calories, faiss_sim, emb_pos, ...)
    cands = RET.search(qv, k=K_SEARCH)

    # 3) Áp dụng filter cứng cho Main/Side dựa trên bối cảnh (allergen, thiết bị, calo,...)
    base_ctx = dict(
        main_ings=req.main_ingredients or [],          # Nguyên liệu mong muốn cho món chính
        avoid_allergens=req.avoid_allergens or [],     # Danh sách allergen cần tránh
        available_equipment=req.available_equipment or [],  # Thiết bị bếp hiện có
        group_rates=req.group_allergy_rates or {},     # Tỉ lệ dị ứng trong nhóm (nếu có)
    )

    # Lọc ra các ứng viên món chính
    mains = cands[cands["DishType"].eq("Main")].copy()
    ctx_main = dict(base_ctx, max_cal=req.max_cal_main)  # Thêm giới hạn calo riêng cho Main
    mains = hard_filter(mains, ctx_main)                 # Lọc cứng theo context món chính

    # Lọc ra các ứng viên món phụ
    sides = cands[cands["DishType"].eq("Side")].copy()
    ctx_side = dict(base_ctx, max_cal=req.max_cal_side)  # Thêm giới hạn calo riêng cho Side
    sides = hard_filter(sides, ctx_side)                 # Lọc cứng theo context món phụ

    # Nếu cả hai đều rỗng sau khi filter -> không có món phù hợp
    if mains.empty and sides.empty:
        return {"main": [], "side": [], "why": "Không có món phù hợp theo tiêu chí."}

    # 4) Rerank + MMR cho hai nhóm main/side
    out = {"main": [], "side": [], "why": ""}           # Khởi tạo cấu trúc trả về

    out["main"] = _do_block(mains, req.top_n_main, ctx_main)  # Chọn danh sách món chính cuối cùng
    out["side"] = _do_block(sides, req.top_n_side, ctx_side)  # Chọn danh sách món phụ cuối cùng

    # 5) Giải thích ngắn gọn lý do (có thể sau này thay bằng LLM để giải thích chi tiết hơn)
    out["why"] = (
        "Gợi ý dựa trên FAISS (ngữ nghĩa), lọc ràng buộc (dị ứng/calo/thiết bị), "
        "rerank (ML) + đa dạng hoá (MMR)."
    )

    return out                                         # Trả về RecommendResponse (pydantic sẽ validate/serialize)
