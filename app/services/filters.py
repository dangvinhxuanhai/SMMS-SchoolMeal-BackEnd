# app/services/filters.py
# Chức năng:
# - hard_filter: áp dụng các rule "cứng" để loại bỏ món không phù hợp trước khi rerank:
#       + Loại món chứa allergen bị cấm.
#       + Loại món vượt quá giới hạn calo.
#       + Loại món yêu cầu thiết bị không có.
# - ingredient_match: tiện ích so khớp nguyên liệu (dùng cho rule mềm / tìm kiếm, nếu cần).

import pandas as pd

def hard_filter(df: pd.DataFrame, ctx) -> pd.DataFrame:
    """
    Lọc cứng danh sách ứng viên theo các ràng buộc trong ctx.

    ctx kỳ vọng chứa:
        - avoid_allergens: list[str] allergen cần tránh tuyệt đối.
        - max_cal: int, giới hạn calo tối đa cho món (theo nhóm main/side).
        - available_equipment: list[str], thiết bị bếp hiện có.

    Quy tắc:
        1. Nếu món có ANY allergen thuộc avoid_allergens -> loại.
        2. Nếu Calories > max_cal -> loại.
        3. Nếu món cần thiết bị nằm ngoài available_equipment -> loại.
    """
    f = df.copy()

    # 1) Loại theo allergen cứng (hard block):
    if ctx["avoid_allergens"]:
        avoid = {a.lower() for a in ctx["avoid_allergens"]}

        def ok(allergens):
            # allergens có thể là list hoặc None
            s = {str(x).lower() for x in (allergens or [])}
            # hợp giữa s & avoid phải rỗng (không được chứa allergen cấm)
            return len(s & avoid) == 0

        f = f[f["Allergens"].apply(ok)]

    # 2) Lọc theo giới hạn calo:
    # Giữ lại món có Calories <= max_cal
    f = f[f["Calories"] <= ctx["max_cal"]]

    # 3) Lọc theo thiết bị bếp:
    if ctx.get("available_equipment"):
        have = set(ctx["available_equipment"])

        def eq_ok(eq):
            # eq: danh sách thiết bị mà món cần; phải là tập con của have
            return set(eq or []).issubset(have)

        f = f[f["Equipment"].apply(eq_ok)]

    return f


def ingredient_match(recipe_ings, targets, require_all: bool = False, ratio_thresh: float = 0.5) -> bool:
    """
    Kiểm tra mức độ khớp giữa nguyên liệu của món (recipe_ings) và danh sách target (targets).

    Tham số:
        recipe_ings : list nguyên liệu của món.
        targets     : list nguyên liệu mong muốn.
        require_all : nếu True -> yêu cầu món chứa TẤT CẢ target (AND).
        ratio_thresh: nếu require_all=False:
                        - chấp nhận nếu:
                            + có ít nhất 1 nguyên liệu giao, HOẶC
                            + tỉ lệ giao >= ratio_thresh.

    Trả về:
        True nếu món "đủ khớp" theo rule trên, False nếu không.

    Thích hợp dùng cho:
        - Rule mềm khi gợi ý món theo nguyên liệu có sẵn.
        - Các bộ lọc bổ sung ngoài hard_filter.
    """
    # Chuẩn hoá về set chữ thường, bỏ khoảng trắng
    r = {str(x).strip().lower() for x in (recipe_ings or [])}
    t = {str(x).strip().lower() for x in (targets or [])}

    # Nếu không có target -> luôn match
    if not t:
        return True

    inter = len(r & t)

    if require_all:
        # Yêu cầu món chứa đầy đủ tất cả target
        return inter == len(t)  # AND

    # Rule mềm:
    # - Match nếu có ít nhất 1 giao,
    #   hoặc tỉ lệ giao / tổng target >= ratio_thresh.
    return inter >= 1 or inter / (len(t) or 1) >= ratio_thresh
