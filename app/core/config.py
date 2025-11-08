# app/core/config.py
# File cấu hình trung tâm cho Kitchen Recommender:
# - Định nghĩa các đường dẫn tài nguyên (CSV, FAISS index, Graph, model ML)
# - Đọc giá trị từ biến môi trường nếu có (để dễ deploy nhiều môi trường)
# - Cung cấp các hằng số cho pipeline: embed model, loại index, K search, top K trả về, bật/tắt ML ranker

import os

# BASE_DIR: thư mục gốc của project (đi lên 2 cấp từ vị trí file config.py)
BASE_DIR = os.path.abspath(
    os.path.join(os.path.dirname(__file__), "../..")
)

# Đường dẫn file CSV chứa danh sách công thức/recipe.
# Ưu tiên đọc từ biến môi trường RECIPES_CSV, nếu không có thì dùng đường dẫn mặc định trong thư mục /data.
DATA_CSV = os.getenv("RECIPES_CSV", f"{BASE_DIR}/data/recipes.csv")

# Đường dẫn file FAISS index để search semantic.
# Có thể override bằng biến môi trường RECIPES_INDEX.
INDEX_PATH = os.getenv("RECIPES_INDEX", f"{BASE_DIR}/index/recipes.index")

# Đường dẫn file graph (đồ thị bếp, nguyên liệu, allergen, ...) ở dạng gpickle.
# Có thể override qua GRAPH_PATH.
GRAPH_PATH = os.getenv("GRAPH_PATH", f"{BASE_DIR}/graph/kitchen_graph.gpickle")

# Tên model embedding dùng để encode câu query và/hoặc recipe text.
# Mặc định: paraphrase-multilingual-MiniLM-L12-v2 (đa ngôn ngữ, nhẹ, nhanh).
EMBED_PROVIDER = os.getenv("EMBED_PROVIDER", "local")  # "local" hoặc "openai"
EMBED_MODEL = os.getenv("EMBED_MODEL", "paraphrase-multilingual-MiniLM-L12-v2")

# Loại FAISS index sử dụng:
# - flat  : duyệt tuyến tính, chính xác, phù hợp dataset nhỏ/vừa
# - ivf   : inverted file, nhanh hơn cho dataset lớn
# - hnsw  : đồ thị nhỏ thế giới, ANN nhanh
INDEX_TYPE = os.getenv("INDEX_TYPE", "flat")

# Số lượng món tối đa trả về cho client (top-N cuối cùng).
TOPK_RETURN = int(os.getenv("TOPK_RETURN", 5))

# Số lượng ứng viên K lấy ra từ FAISS trước khi filter + rerank.
K_SEARCH = int(os.getenv("K_SEARCH", 100))

# Cờ bật/tắt dùng ML Ranker:
# - Nếu USE_ML = "1" trong env -> True (dùng model ML để rerank)
# - Ngược lại -> False (fallback rule-based/score khác)
USE_ML = os.getenv("USE_ML", "0") == "1"

# Đường dẫn file .pkl của ML Ranker:
# - Dùng khi USE_ML=True
# - Cho phép override bằng biến môi trường RANKER_PKL
RANKER_PKL = os.getenv(
    "RANKER_PKL",
    f"{BASE_DIR}/app/models/ranker.pkl"
)
