from fastapi import APIRouter
from pydantic import BaseModel, Field
from typing import List, Optional
from src.app.services.retriever import Retriever
from src.app.services.ranker import get_item_sets, rank

router = APIRouter()
retriever = Retriever()

class RecommendIn(BaseModel):
    main_ingredients: List[str] = Field(default_factory=list)
    avoid_allergens: List[str] = Field(default_factory=list)
    need_ingredients: List[str] = Field(default_factory=list)
    k: int = 15

@router.post("/recommend")
def recommend(payload: RecommendIn):
    qtxt = (
        f"Main ingredients: {', '.join(payload.main_ingredients)}. "
        f"Avoid allergens: {', '.join(payload.avoid_allergens)}. "
        f"Need ingredients: {', '.join(payload.need_ingredients)}."
    )
    cand_df, rowids, sims = retriever.search(qtxt, k=max(100, payload.k*8))
    meta_df = get_item_sets(cand_df["food_id"].tolist())
    ranked = rank(
        cand_df, meta_df,
        main_ingredients=[x.lower() for x in payload.main_ingredients],
        need_ingredients=[x.lower() for x in payload.need_ingredients],
        avoid_allergens=[x.lower() for x in payload.avoid_allergens]
    )
    out = []
    for _,r in ranked.head(payload.k).iterrows():
        out.append({
            "food_id": int(r.food_id),
            "name": r.name,
            "calories": int(r.calories) if r.calories is not None else None,
            "allergens": r.allergens,
            "main_ingredients": r.main_ings,
            "score": round(float(r.score), 4),
            "features": {
                "semantic_sim": round(float(r.semantic_sim),4),
                "ingredient_overlap": round(float(r.ingredient_overlap),4),
                "popularity_norm": round(float(r.pop_norm),4),
                "need_bonus": round(float(r.need_bonus),4)
            }
        })
    return {
        "recommendations": out,
        "meta": {
            "retrieved": len(cand_df),
            "returned": len(out),
            "model": retriever.model.get_sentence_embedding_dimension(),
            "index_type": "faiss-ip"
        }
    }
