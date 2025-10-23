from pydantic import BaseModel, Field
from typing import List, Dict, Optional

class RecommendRequest(BaseModel):
    query: str = ""
    main_ingredients: List[str] = Field(default_factory=list)
    side_ingredients: List[str] = Field(default_factory=list)
    avoid_allergens: List[str] = Field(default_factory=list)
    available_equipment: List[str] = Field(default_factory=list)
    diners_count: int = 0
    group_allergy_rates: Dict[str, float] = Field(default_factory=dict)
    max_cal_main: int = 600
    max_cal_side: int = 400
    top_n_main: int = 5
    top_n_side: int = 5

class Dish(BaseModel):
    RecipeId: str | int
    Name: str
    Calories: float
    Allergens: list[str] | str

class RecommendResponse(BaseModel):
    main: list[Dish] = Field(default_factory=list)
    side: list[Dish] = Field(default_factory=list)
    why: str = ""
