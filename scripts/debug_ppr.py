# scripts/debug_ppr.py
# Mục đích:
# - Chạy 1 query giả định
# - Lấy các ứng viên từ FAISS
# - Build features (có ppr)
# - In ra thống kê cột ppr để xem có toàn 0 không
import os
import sys

# Xác định đường dẫn thư mục gốc project
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# Thêm BASE_DIR vào sys.path để Python tìm được package "app"
if BASE_DIR not in sys.path:
    sys.path.insert(0, BASE_DIR)

import pandas as pd

from app.core.config import DATA_CSV, EMBED_MODEL, INDEX_PATH, GRAPH_PATH
from app.services.embedder import Embedder
from app.services.retrieval import FaissRetriever
from app.services.graph import KitchenGraph
from app.services.features import build_features


def main():
    print("[debug_ppr] Load resources...")

    # 1. Load data & services giống như trong app
    df = pd.read_csv(DATA_CSV)
    emb = Embedder(EMBED_MODEL)
    ret = FaissRetriever(INDEX_PATH, df)
    kg = KitchenGraph(GRAPH_PATH)
    print("[debug_ppr] GRAPH_PATH =", GRAPH_PATH)
    print("[debug_ppr] #nodes:", kg.G.number_of_nodes(), "#edges:", kg.G.number_of_edges())

    # Lấy một ít node Ingredient để xem format thực tế
    ing_nodes = [n for n in kg.G.nodes if isinstance(n, tuple) and n[0] == "Ingredient"]
    print("[debug_ppr] Sample Ingredient nodes:", ing_nodes[:30])

    # Test trực tiếp xem node ('Ingredient', 'gà') có tồn tại không
    print("[debug_ppr] Has ('Ingredient', 'gà'):", ("Ingredient", "gà") in kg.G)


    # 2. Tạo một context test
    #    Bạn có thể đổi "chicken", "egg" thành nguyên liệu hay dùng trong dataset của bạn.
    # main_ings = ["ức gà", "cá hồi"]
    main_ings = ["gà"]

    # Test cho từng seed bạn dùng
    for ing in main_ings:
        node = ("Ingredient", ing.lower())
        print(f"[debug_ppr] Seed {ing} -> node {node} exists?", node in kg.G)
        
    ctx = {
        "main_ings": main_ings,
        "avoid_allergens": [],
        "available_equipment": [],
        "group_rates": {},
        "max_cal": 600,
    }

    print(f"[debug_ppr] Test with main_ings = {main_ings}")

    # 3. Tạo query text & embedding
    qtxt = " ".join(main_ings)
    qv = emb.encode([qtxt])

    # 4. Lấy candidates từ FAISS (ví dụ 50 món đầu)
    cands = ret.search(qv, k=50)

    if cands.empty:
        print("[debug_ppr] Không lấy được candidate nào từ FAISS. Kiểm tra lại index/embedding.")
        return

    # 5. Build features (gồm ppr)
    feat = build_features(cands, ctx, kg)

    # 6. In kết quả ppr
    print("\n[debug_ppr] Sample ppr values (top 20 rows):")
    print(feat[["RecipeId", "ppr"]].head(20))

    print("\n[debug_ppr] ppr statistics:")
    print(feat["ppr"].describe())

    # 7. Đánh giá nhanh
    if (feat["ppr"] == 0).all():
        print("\n[debug_ppr] ⚠ ppr toàn 0 ⇒ Personalized PageRank hiện đang không có tác dụng.")
    else:
        print("\n[debug_ppr] ✅ ppr có giá trị khác 0 ⇒ Personalized PageRank đang hoạt động.")


if __name__ == "__main__":
    main()
