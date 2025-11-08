# app/services/embedder.py
# Service phụ trách:
# - Load SentenceTransformer model (theo tên cấu hình EMBED_MODEL)
# - Cung cấp hàm encode() để chuyển text/query thành vector embedding chuẩn hóa (float32)
# - Dùng chung cho FAISS search & các bước downstream khác để tránh load model nhiều lần

from sentence_transformers import SentenceTransformer  # Thư viện tạo sentence embeddings
import numpy as np                                     # Dùng để xử lý và ép kiểu mảng vector

class Embedder:
    def __init__(self, model_name: str):
        # Khởi tạo: load model embedding theo tên (vd: paraphrase-multilingual-MiniLM-L12-v2)
        # Model này chỉ được load 1 lần và tái sử dụng xuyên suốt app.
        self.model = SentenceTransformer(model_name)

    def encode(self, texts: list[str]) -> np.ndarray:
        """
        Nhận vào danh sách chuỗi (query, mô tả recipe, nguyên liệu, ...) và trả về:
        - Ma trận embedding dạng numpy.ndarray shape = (len(texts), dim)
        - Đã normalize (norm = 1) để dùng cosine similarity 1 cách nhất quán.
        - Ép kiểu float32 để:
          + Tiết kiệm RAM
          + Tương thích với FAISS & các tính toán vector hiệu quả hơn.
        """
        # convert_to_numpy=True: trả về numpy array
        # normalize_embeddings=True: chuẩn hóa vector về đơn vị (giúp cosine = dot product)
        X = self.model.encode(texts, convert_to_numpy=True, normalize_embeddings=True)

        # Ép kiểu về float32 (mặc định nhiều model trả về float64)
        return X.astype("float32")
