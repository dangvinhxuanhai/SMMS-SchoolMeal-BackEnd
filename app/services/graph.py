# app/services/graph.py
# Service phụ trách làm việc với Kitchen Graph:
# - Load đồ thị kiến thức (nguyên liệu, allergen, công thức, quan hệ liên quan)
# - Cung cấp các hàm tính feature/score dựa trên graph:
#     + ingredient_coverage: mức độ khớp nguyên liệu
#     + allergen_risk: mức độ rủi ro dị ứng (cứng + mềm)
#     + ppr_score: độ liên quan của món ăn với các nguyên liệu seed bằng Personalized PageRank

import os               # Kiểm tra file graph tồn tại
import math             # (hiện chưa dùng, có thể dùng cho các tính toán mở rộng)
import importlib        # Dùng để import module gpickle linh hoạt theo version networkx
import networkx as nx   # Thư viện xử lý graph (NetworkX)

def _read_gpickle(path: str):
    """
    Đọc graph từ file .gpickle theo cách tương thích:
    - Ưu tiên dùng networkx.readwrite.gpickle.read_gpickle (API mới hơn).
    - Nếu lỗi (do version khác nhau) thì fallback về pickle.load thủ công.
    Giúp không bị vỡ khi deploy ở môi trường có networkx version khác.
    """
    try:
        # Thử import module con readwrite.gpickle của networkx (cách chính thống)
        nx_gpickle = importlib.import_module("networkx.readwrite.gpickle")
        return nx_gpickle.read_gpickle(path)    # Đọc graph bằng hàm của networkx
    except Exception:
        # Nếu không có module/hàm phù hợp -> dùng pickle trực tiếp (tương thích backward)
        import pickle
        with open(path, "rb") as f:
            return pickle.load(f)               # Load object graph từ file nhị phân


class KitchenGraph:
    def __init__(self, path_gpickle: str):
        # Nếu file graph không tồn tại -> báo lỗi rõ ràng, tránh silent bug.
        if not os.path.exists(path_gpickle):
            raise FileNotFoundError(f"Graph file not found: {path_gpickle}")
        # Đọc graph và lưu vào self.G để tái sử dụng cho các hàm tính toán.
        self.G = _read_gpickle(path_gpickle)

    # ---------- Các hàm chấm điểm/feature trên graph ----------

    @staticmethod
    def ingredient_coverage(recipe_ings, target_ings) -> float:
        """
        Tính độ phủ nguyên liệu: trong các nguyên liệu target_ings mà user mong muốn,
        có bao nhiêu nguyên liệu xuất hiện trong recipe_ings.

        Trả về giá trị từ 0.0 -> 1.0:
        - 1.0: recipe chứa toàn bộ nguyên liệu target
        - 0.0: không khớp nguyên liệu nào

        Đã normalize về lowercase và handle None/empty.
        """
        # Chuyển danh sách nguyên liệu món thành set chữ thường
        s = {str(x).lower() for x in (recipe_ings or [])}
        # Chuyển danh sách nguyên liệu mục tiêu thành set chữ thường
        t = {str(x).lower() for x in (target_ings or [])}
        # Tính tỉ lệ giao / kích thước target (nếu target rỗng thì chia cho 1 để tránh lỗi)
        return len(s & t) / (len(t) or 1)

    @staticmethod
    def allergen_risk(
        recipe_allergens,
        avoid_allergens,
        group_rates: dict[str, float] = None
    ) -> float:
        """
        Tính điểm rủi ro dị ứng cho một món ăn.

        Hai phần:
        - hard (cực mạnh): nếu món có allergen nằm trong avoid_allergens -> cộng 10 điểm.
        - soft (mềm): cộng thêm tổng tỉ lệ dị ứng của các allergen trong nhóm (group_rates).

        Công thức:
            risk = 10 * hard + soft
        -> Nếu có allergen bị cấm thì luôn rất rủi ro (ưu tiên loại bỏ).
        """
        group_rates = group_rates or {}
        # Tập allergen của món (lowercase)
        a = {str(x).lower() for x in (recipe_allergens or [])}
        # Tập allergen cần tránh (lowercase)
        b = {str(x).lower() for x in (avoid_allergens or [])}

        # hard = 1 nếu có giao giữa a và b, ngược lại = 0
        hard = 1.0 if (a & b) else 0.0

        # soft = tổng tỉ lệ dị ứng trong group cho các allergen xuất hiện trong món
        soft = sum(float(group_rates.get(x, 0.0)) for x in a)

        # hard được nhân 10 để luôn áp đảo soft (ưu tiên loại món vi phạm cứng)
        return 10.0 * hard + soft  # hard >> soft

    def ppr_score(self, seed_ings, recipe_node: tuple, alpha: float = 0.85) -> float:
        """
        Tính Personalized PageRank score cho một node món ăn so với các seed_ings.

        - seed_ings: danh sách node "nguồn" (thường là các nguyên liệu mà user quan tâm).
                     Chỉ giữ những seed có tồn tại trong graph.
        - recipe_node: định danh node món ăn cần đo (vd: ("Recipe", recipe_id)).
        - alpha: hệ số damping cho PageRank (mặc định 0.85).

        Ý nghĩa:
        - Score càng cao -> món càng "gần" / "liên quan" tới các nguyên liệu đầu vào
          trên đồ thị kiến thức (ingredient → recipe → allergen → ...).
        """
        # # Tạo personalization vector: mỗi seed xuất hiện trong graph được set weight = 1.0
        # seeds = {
        #     str(n): 1.0
        #     for n in (seed_ings or [])
        #     if (str(n) in self.G)
        # }
        # # Nếu không có seed hợp lệ -> không tính được, trả về 0
        # if not seeds:
        #     return 0.0

        # # Chạy Personalized PageRank trên đồ thị:
        # # - alpha: xác suất tiếp tục random walk
        # # - personalization: bias random walk về các seed
        # # - max_iter: giới hạn vòng lặp để tránh treo
        # pr = nx.pagerank(
        #     self.G,
        #     alpha=alpha,
        #     personalization=seeds,
        #     max_iter=50
        # )

        # # Lấy score của node món ăn; nếu không tồn tại thì trả 0.0
        # return float(pr.get(recipe_node, 0.0))

        # Normalize recipe_node id về string
        if recipe_node is None:
            return 0.0
        try:
            t, rid = recipe_node
            recipe_node = (t, str(rid))
        except Exception:
            return 0.0
        
        seeds = {}
        for n in (seed_ings or []):
            ing_node = ("Ingredient", str(n).lower())
            if ing_node in self.G:
                seeds[ing_node] = 1.0
                
        print("[ppr_score] seeds:", list(seeds.keys())[:10])
        print("[ppr_score] has recipe_node:", recipe_node in self.G)

        if not seeds:
            return 0.0

        pr = nx.pagerank(
            self.G,
            alpha=alpha,
            personalization=seeds,
            max_iter=50,
        )

        return float(pr.get(recipe_node, 0.0))

