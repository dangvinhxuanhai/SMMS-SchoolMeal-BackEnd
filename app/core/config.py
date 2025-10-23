import os
BASE_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))

DATA_CSV = os.getenv("RECIPES_CSV", f"{BASE_DIR}/data/recipes.csv")
INDEX_PATH = os.getenv("RECIPES_INDEX", f"{BASE_DIR}/index/recipes.index")
GRAPH_PATH = os.getenv("GRAPH_PATH", f"{BASE_DIR}/graph/kitchen_graph.gpickle")

EMBED_MODEL = os.getenv("EMBED_MODEL", "paraphrase-multilingual-MiniLM-L12-v2")
INDEX_TYPE  = os.getenv("INDEX_TYPE", "flat")     # flat|ivf|hnsw
TOPK_RETURN = int(os.getenv("TOPK_RETURN", 5))
K_SEARCH    = int(os.getenv("K_SEARCH", 100))
USE_ML      = os.getenv("USE_ML", "0") == "1"     # dùng ranker ML hay chưa
RANKER_PKL  = os.getenv("RANKER_PKL", f"{BASE_DIR}/app/models/ranker.pkl")
