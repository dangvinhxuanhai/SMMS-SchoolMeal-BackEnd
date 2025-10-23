# app/api/main.py
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

# dùng import tương đối
from .routers.menu import router as menu_router

app = FastAPI(title="Kitchen Recommender API")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(menu_router)

@app.get("/")
def root():
    return {"ok": True, "service": "kitchen-recommender"}

@app.get("/health")
def health():
    return {"status": "up"}
