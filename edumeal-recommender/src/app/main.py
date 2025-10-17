from fastapi import FastAPI
from src.app.api.recommend import router as recommend_router

app = FastAPI(title="EduMeal Recommender (RAG)")
app.include_router(recommend_router, prefix="")

@app.get("/")
def hello():
    return {"msg": "OK"}
