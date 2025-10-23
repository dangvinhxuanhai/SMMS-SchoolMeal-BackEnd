# -*- coding: utf-8 -*-
"""
Build embeddings + FAISS index cho recipes.
Hỗ trợ:
- Local embedding: sentence-transformers (mặc định, không cần API key)
- (Tuỳ chọn) Cloud embedding: OpenAI (cần OPENAI_API_KEY), --use_openai
- Index: Flat (exact), IVF (ANN), HNSW (ANN)
Chuẩn hoá L2 để xếp hạng theo cosine similarity qua Inner Product.
"""

import os, json, argparse, math, time
import numpy as np
import pandas as pd
import faiss

# --------------------------
# Helpers
# --------------------------
def parse_list(x):
    """
    Chuẩn hoá field list: nhận JSON list hoặc chuỗi phân cách bằng dấu phẩy.
    Trả: list[str]
    """
    if x is None or (isinstance(x, float) and math.isnan(x)):
        return []
    if isinstance(x, list):
        return [str(i).strip() for i in x if str(i).strip()]
    s = str(x).strip()
    if not s:
        return []
    # thử parse JSON
    try:
        j = json.loads(s)
        if isinstance(j, list):
            return [str(i).strip() for i in j if str(i).strip()]
    except Exception:
        pass
    # fallback: tách phẩy
    parts = [p.strip() for p in s.split(",")]
    return [p for p in parts if p]

def make_doc_text(row):
    """
    Ghép các trường làm doc_text cho embedding.
    Bạn có thể tuỳ biến theo cột thực tế của recipes.csv.
    """
    parts = []
    for col in ["Name", "Description"]:
        if col in row and str(row[col]).strip():
            parts.append(str(row[col]))
    if "Ingredients" in row:
        ings = parse_list(row["Ingredients"])
        if ings:
            parts.append("Ingredients: " + ", ".join(ings))
    if "Allergens" in row:
        allergens = parse_list(row["Allergens"])
        if allergens:
            parts.append("Allergens: " + ", ".join(allergens))
    # dinh dưỡng
    for col in ["Calories", "Protein", "Fat", "Carbs"]:
        if col in row and str(row[col]).strip():
            parts.append(f"{col}: {row[col]}")
    if "Cuisine" in row and str(row["Cuisine"]).strip():
        parts.append("Cuisine: " + str(row["Cuisine"]))
    if "Season" in row and str(row["Season"]).strip():
        parts.append("Season: " + str(row["Season"]))
    return " | ".join(parts)

def l2_normalize(X: np.ndarray, eps=1e-12) -> np.ndarray:
    n = np.linalg.norm(X, axis=1, keepdims=True)
    return X / (n + eps)

def embed_local(texts, model_name: str, batch_size: int = 64) -> np.ndarray:
    from sentence_transformers import SentenceTransformer
    model = SentenceTransformer(model_name)
    X = model.encode(texts, convert_to_numpy=True, normalize_embeddings=True, batch_size=batch_size)
    return X.astype("float32")

def embed_openai(texts, openai_model: str) -> np.ndarray:
    from openai import OpenAI
    client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))
    # chunk vì API giới hạn
    vecs = []
    B = 128
    for i in range(0, len(texts), B):
        chunk = texts[i:i+B]
        resp = client.embeddings.create(model=openai_model, input=chunk)
        vecs.extend([d.embedding for d in resp.data])
    X = np.array(vecs, dtype="float32")
    X = l2_normalize(X)
    return X

def build_index_flat(X: np.ndarray):
    d = X.shape[1]
    index = faiss.IndexFlatIP(d)
    index.add(X)
    return index

def build_index_ivf(X: np.ndarray, nlist=1024, nprobe=16):
    d = X.shape[1]
    quantizer = faiss.IndexFlatIP(d)
    index = faiss.IndexIVFFlat(quantizer, d, nlist, faiss.METRIC_INNER_PRODUCT)
    index.train(X)              # bắt buộc
    index.add(X)
    index.nprobe = int(nprobe)
    return index

def build_index_hnsw(X: np.ndarray, M=32, efC=200, efS=64):
    d = X.shape[1]
    index = faiss.IndexHNSWFlat(d, M)
    # metric mặc định L2; vì X đã normalize, L2 ~ cosine; OK
    # Nếu muốn IP chuẩn cho HNSW, cần chuyển metric, nhưng FlatHNSW mặc định là L2.
    ps = faiss.ParameterSpace()
    ps.set_index_parameter(index, "efConstruction", int(efC))
    index.add(X)
    ps.set_index_parameter(index, "efSearch", int(efS))
    return index

# --------------------------
# Main
# --------------------------
def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes.csv")
    ap.add_argument("--out_csv", default="data/recipes_with_text.csv")
    ap.add_argument("--embeddings_npy", default="index/recipe_embeddings.npy")
    ap.add_argument("--index_out", default="index/recipes.index")
    ap.add_argument("--metadata_out", default="index/metadata.json")
    ap.add_argument("--model", default="paraphrase-multilingual-MiniLM-L12-v2")
    ap.add_argument("--use_openai", action="store_true")
    ap.add_argument("--openai_model", default="text-embedding-3-small")
    ap.add_argument("--index_type", choices=["flat","ivf","hnsw"], default="flat")
    ap.add_argument("--nlist", type=int, default=1024)
    ap.add_argument("--nprobe", type=int, default=16)
    ap.add_argument("--M", type=int, default=32)
    ap.add_argument("--efC", type=int, default=200)
    ap.add_argument("--efS", type=int, default=64)
    ap.add_argument("--batch_size", type=int, default=64)
    args = ap.parse_args()

    os.makedirs(os.path.dirname(args.out_csv), exist_ok=True)
    os.makedirs(os.path.dirname(args.embeddings_npy), exist_ok=True)
    os.makedirs(os.path.dirname(args.index_out), exist_ok=True)

    print(f"[build_index] Load: {args.recipes_csv}")
    df = pd.read_csv(args.recipes_csv)

    # Chuẩn hoá cột cơ bản
    if "RecipeId" not in df.columns:
        df["RecipeId"] = df.index.astype(str)
    df["Name"] = df.get("Name", df["RecipeId"]).astype(str)
    for col in ["Calories","Protein","Fat","Carbs"]:
        if col in df.columns:
            df[col] = pd.to_numeric(df[col], errors="coerce").fillna(0).astype(float)
    # list-like
    for col in ["Ingredients","Allergens","Equipment"]:
        if col in df.columns:
            df[col] = df[col].apply(parse_list)
        else:
            df[col] = [[] for _ in range(len(df))]
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"] * len(df)  # default

    # doc_text
    print("[build_index] Build doc_text ...")
    df["doc_text"] = df.apply(make_doc_text, axis=1)

    # Embedding
    print(f"[build_index] Embedding ({'OpenAI' if args.use_openai else 'local'}) ...")
    texts = df["doc_text"].tolist()
    if args.use_openai:
        X = embed_openai(texts, args.openai_model)
        emb_meta = {"provider":"openai","model":args.openai_model}
    else:
        X = embed_local(texts, args.model, batch_size=args.batch_size)
        emb_meta = {"provider":"sbert","model":args.model}
    # X đã normalize nếu local (normalize_embeddings=True); openai đã normalize ở hàm

    # Lưu embeddings (tuỳ chọn)
    np.save(args.embeddings_npy, X)

    N = X.shape[0]

    print(f"[build_index] rows={N}, dim={X.shape[1]}")
    if args.index_type == "ivf":
        eff_nlist = max(1, min(args.nlist, N))
        eff_nprobe = max(1, min(args.nprobe, eff_nlist))
        if eff_nlist != args.nlist:
            print(f"[warn] nlist={args.nlist} > N={N} ⇒ dùng nlist={eff_nlist}")
        if eff_nprobe != args.nprobe:
            print(f"[warn] nprobe={args.nprobe} > nlist={eff_nlist} ⇒ dùng nprobe={eff_nprobe}")
        # mẹo: nếu N nhỏ, IVF không có lợi
        if N < 1000:
            print("[warn] N nhỏ (<1000): IVF thường không lợi; cân nhắc dùng --index_type flat")
        index = build_index_ivf(X, nlist=eff_nlist, nprobe=eff_nprobe)

    # Build index
    print(f"[build_index] Build index type={args.index_type} ...")
    if args.index_type == "flat":
        index = build_index_flat(X)
    elif args.index_type == "ivf":
        index = build_index_ivf(X, nlist=args.nlist, nprobe=args.nprobe)
    else:
        index = build_index_hnsw(X, M=args.M, efC=args.efC, efS=args.efS)

    # Lưu index & CSV đã có doc_text
    faiss.write_index(index, args.index_out)
    df.to_csv(args.out_csv, index=False)

    meta = {
        "built_at": time.strftime("%Y-%m-%d %H:%M:%S"),
        "index_type": args.index_type,
        "index_out": args.index_out,
        "recipes_csv": args.recipes_csv,
        "rows": int(len(df)),
        "dim": int(X.shape[1]),
        "embedding": emb_meta,
        "ivf": {"nlist": args.nlist, "nprobe": args.nprobe} if args.index_type=="ivf" else None,
        "hnsw": {"M": args.M, "efC": args.efC, "efS": args.efS} if args.index_type=="hnsw" else None,
    }
    with open(args.metadata_out, "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)

    print("[build_index] Done.")
    print(json.dumps(meta, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
