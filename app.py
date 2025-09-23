from fastapi import FastAPI
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
    api_key="tao-secret-api-key-roi-dan-vao-day"
    #   https://platform.openai.com/api-keys
    #   em dùng link trên để tạo API key
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
def recommend(request: RecommendRequest):
    query = request.query
    max_calories = request.max_calories
    avoid_allergens = request.avoid_allergens or []

    # Prompt có chữ "JSON"
    prompt = f"""
    Bạn là một chuyên gia dinh dưỡng. Hãy trả lời câu hỏi sau của người dùng bằng JSON.

    Yêu cầu: {query}
    Giới hạn: tối đa {max_calories} kcal
    Tránh các chất gây dị ứng: {", ".join(avoid_allergens)}

    Trả về JSON có dạng:
    {{
        "recommendations": [
            {{
                "Name": "Tên món ăn",
                "Calories": <số>,
                "Allergens": ["danh sách"]
            }}
        ],
        "ai_summary": "Tóm tắt bằng tiếng Việt"
    }}
    """

    response = client.chat.completions.create(
        model="gpt-4o-mini",
        messages=[{"role": "user", "content": prompt}],
        response_format={"type": "json_object"},
    )

    # Parse JSON từ response
    ai_output = response.choices[0].message.content
    return json.loads(ai_output)
