import os
from pathlib import Path
from typing import cast

import faiss
import numpy as np
import pandas as pd
from dotenv import load_dotenv
from sentence_transformers import SentenceTransformer
from sqlalchemy import create_engine, text

# ── Load .env
load_dotenv()

DATA_CSV = os.getenv("DATA_CSV", "data/recipes_text.csv")
INDEX_PATH = os.getenv("FAISS_INDEX_PATH", "data/recipes.index")
MODEL_NAME = os.getenv("EMBED_MODEL", "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2")
DATABASE_URL = os.getenv("DATABASE_URL")

if not DATABASE_URL:
    raise ValueError("DATABASE_URL is missing in .env")
if not INDEX_PATH:
    raise ValueError("FAISS_INDEX_PATH is missing in .env")

# ── Ensure folders exist
Path(os.path.dirname(INDEX_PATH)).mkdir(parents=True, exist_ok=True)
Path("data").mkdir(parents=True, exist_ok=True)

# ── Read data to embed
if not os.path.exists(DATA_CSV):
    raise FileNotFoundError(f"Missing input: {DATA_CSV}. Run scripts/prepare_text.py first.")

df = pd.read_csv(DATA_CSV)
req_cols = {"food_id", "doc_text"}
missing = req_cols - set(df.columns)
if missing:
    raise ValueError(f"data/recipes_text.csv missing columns: {missing}")

texts = df["doc_text"].astype(str).tolist()

# ── Build embeddings (float32 + normalized)
print(f"🔹 Loading model: {MODEL_NAME}")
model = SentenceTransformer(MODEL_NAME)
emb = model.encode(texts, convert_to_numpy=True, normalize_embeddings=True).astype(np.float32)

if emb.ndim != 2:
    raise ValueError(f"Embeddings must be 2D (N, D); got shape {emb.shape}")
n, dim = emb.shape
print(f"✅ Created {n} vectors (dim={dim})")

# ── Build FAISS index (Inner Product / cosine-like)
index = faiss.IndexFlatIP(dim)

# 👉 FIX PYLANCE: ép kiểu float32 + contiguous + ignore type cho call vào C-extension
x = np.ascontiguousarray(emb, dtype=np.float32)
index.add(cast(np.ndarray, x))  # type: ignore[arg-type]

# ── Save artifacts
faiss.write_index(index, INDEX_PATH)
np.save("data/embeddings.npy", emb)
print(f"💾 Saved FAISS index → {INDEX_PATH}")
print("💾 Saved embeddings → data/embeddings.npy")

# ── Map FAISS rowid ↔ food_id
engine = create_engine(DATABASE_URL, pool_pre_ping=True)
print("🔹 Writing rowid ↔ food_id mapping ...")
with engine.begin() as conn:
    conn.execute(text("TRUNCATE TABLE recipe_embeddings RESTART IDENTITY"))
    rows = [{"fid": int(df.food_id[i]), "rid": int(i)} for i in range(n)]
    conn.execute(
        text("INSERT INTO recipe_embeddings (food_id, faiss_rowid) VALUES (:fid, :rid)"),
        rows,
    )
print("✅ Mapping OK")
print(f"🎉 Built FAISS index {INDEX_PATH} with {n} vectors (dim={dim})")


