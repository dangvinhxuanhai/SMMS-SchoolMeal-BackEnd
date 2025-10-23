from fastapi import FastAPI
from fastapi import HTTPException
from pydantic import BaseModel
import pandas as pd
import numpy as np
import faiss
from sentence_transformers import SentenceTransformer
from openai import OpenAI

import json

from pydantic import BaseModel
from typing import List

class RecommendRequest(BaseModel):
    query: str
    max_calories: int
    avoid_allergens: List[str] = []
    
# -----------------------
# Load dữ liệu + FAISS index
# -----------------------
df = pd.read_csv("data/recipes_with_text.csv")
embeddings = np.load("data/recipe_embeddings.npy")

d = embeddings.shape[1]
index = faiss.read_index("data/recipes.index")

# Load embedding model
model = SentenceTransformer("paraphrase-multilingual-MiniLM-L12-v2")

# OpenAI client
client = OpenAI(

)

# -----------------------
# FastAPI setup
# -----------------------
app = FastAPI(title="Meal Recommendation RAG API")


class QueryRequest(BaseModel):
    query: str
    max_calories: int = 500
    avoid_allergens: list[str] = []


# -----------------------
# Helper: filter logic
# -----------------------
def filter_results(results, max_cal=400, avoid_allergens=[]):
    filtered = []
    for _, row in results.iterrows():
        if row["Calories"] <= max_cal:
            if not any(
                a.lower() in str(row["Allergens"]).lower() for a in avoid_allergens
            ):
                filtered.append(row)
    return pd.DataFrame(filtered)


# -----------------------
# API endpoint
# -----------------------
@app.post("/recommend")
def recommend(req: RecommendRequest):
    # 1) FAISS search
    qv = model.encode([req.query], convert_to_numpy=True, normalize_embeddings=True).astype(np.float32)
    D, I = index.search(qv, k=15)
    candidates = df.iloc[I[0]].copy()

    # 2) Lọc
    filtered = filter_results(candidates, req.max_calories, req.avoid_allergens)
    top = filtered.head(5)[["Name","Calories","Allergens"]].to_dict(orient="records")

    # 3) Nếu không có kết quả, trả sớm
    if not top:
        return {"recommendations": [], "ai_summary": "Không tìm thấy món phù hợp theo tiêu chí."}

    # 4) Dùng LLM (tùy chọn). Nếu không có key/quota, có thể bỏ qua và trả trực tiếp.
    try:
        prompt = (
            "Bạn là chuyên gia dinh dưỡng. Dựa trên các ứng viên sau (JSON) hãy trả đúng JSON:\n"
            f"{json.dumps(top, ensure_ascii=False)}\n"
            '{"recommendations":[{"Name":"","Calories":0,"Allergens":[]}], "ai_summary":""}'
        )
        r = client.chat.completions.create(
            model=os.getenv("LLM_MODEL","gpt-4o-mini"),
            messages=[{"role":"user","content":prompt}],
            response_format={"type":"json_object"},
        )
        return json.loads(r.choices[0].message.content)
    except Exception as e:
        # Fallback: không dùng LLM
        return {
            "recommendations": top,
            "ai_summary": f"(FALLBACK) Gợi ý dựa trên FAISS cho yêu cầu: {req.query}"
        }