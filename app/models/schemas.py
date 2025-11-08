# app/models/schemas.py
# Định nghĩa các schema (Pydantic models) cho Kitchen Recommender:
# - RecommendRequest: dữ liệu đầu vào cho API /menu/recommend
# - Dish: cấu trúc 1 món ăn được gợi ý
# - RecommendResponse: dữ liệu trả về gồm danh sách món chính, món phụ và lý do gợi ý

from pydantic import BaseModel, Field
from typing import List, Dict, Optional

class RecommendRequest(BaseModel):
    # Câu truy vấn tự do (vd: "món gà ít dầu cho 30 học sinh")
    query: str = ""
    # Danh sách nguyên liệu mong muốn cho món chính (ưu tiên semantic + filter)
    main_ingredients: List[str] = Field(default_factory=list)
    # Danh sách nguyên liệu mong muốn cho món phụ
    side_ingredients: List[str] = Field(default_factory=list)
    # Danh sách allergen cần tránh (vd: ["milk", "peanut"])
    avoid_allergens: List[str] = Field(default_factory=list)
    # Thiết bị bếp hiện có (vd: ["oven", "steamer"]) để loại bỏ món cần thiết bị không có
    available_equipment: List[str] = Field(default_factory=list)
    # Số lượng người ăn (có thể dùng cho logic sau này: scale khẩu phần, cost, ...)
    diners_count: int = 0
    # Tỉ lệ dị ứng trong nhóm theo từng allergen, vd: {"milk": 0.2, "egg": 0.05}
    # Hỗ trợ ra quyết định món nào an toàn hơn cho tập thể
    group_allergy_rates: Dict[str, float] = Field(default_factory=dict)
    # Giới hạn calo tối đa cho một món chính (per serving)
    max_cal_main: int = 600
    # Giới hạn calo tối đa cho một món phụ
    max_cal_side: int = 400
    # Số lượng món chính muốn lấy sau khi rerank + MMR
    top_n_main: int = 5
    # Số lượng món phụ muốn lấy sau khi rerank + MMR
    top_n_side: int = 5


class Dish(BaseModel):
    # ID công thức/món ăn (có thể là int trong CSV/DB hoặc string)
    RecipeId: str | int
    # Tên món ăn
    Name: str
    # Lượng calo (per serving hoặc theo chuẩn trong dataset)
    Calories: float
    # Thông tin allergen liên quan đến món:
    # - Có thể là list[str] (["milk","egg"])
    # - Hoặc string (nếu dữ liệu nguồn chưa chuẩn hoá)
    Allergens: list[str] | str


class RecommendResponse(BaseModel):
    # Danh sách món chính được gợi ý (sau filter + rerank + MMR)
    main: list[Dish] = Field(default_factory=list)
    # Danh sách món phụ được gợi ý
    side: list[Dish] = Field(default_factory=list)
    # Thông điệp giải thích ngắn gọn cách hệ thống gợi ý (có thể nâng cấp dùng LLM sau)
    why: str = ""
