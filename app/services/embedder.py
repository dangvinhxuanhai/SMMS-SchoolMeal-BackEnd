# app/services/embedder.py
# Service phụ trách:
# - Load SentenceTransformer model (theo tên cấu hình EMBED_MODEL)
# - Cung cấp hàm encode() để chuyển text/query thành vector embedding chuẩn hóa (float32)
# - Dùng chung cho FAISS search & các bước downstream khác để tránh load model nhiều lần

from sentence_transformers import SentenceTransformer  # Thư viện tạo sentence embeddings
import numpy as np                                     # Dùng để xử lý và ép kiểu mảng vector

class Embedder:
    def __init__(self, model_name: str, provider: str = "local"):
        self.provider = provider
        self.model_name = model_name

        if provider == "openai":
            from openai import OpenAI
            api_key = os.getenv("OPENAI_API_KEY")
            if not api_key:
                raise RuntimeError("OPENAI_API_KEY không được set nhưng EMBED_PROVIDER=openai")
            self.client = OpenAI(api_key=api_key)
        else:
            from sentence_transformers import SentenceTransformer
            self.model = SentenceTransformer(model_name)

    def _l2_normalize(self, X: np.ndarray, eps: float = 1e-12) -> np.ndarray:
        n = np.linalg.norm(X, axis=1, keepdims=True)
        return X / (n + eps)

    def encode(self, texts: list[str]) -> np.ndarray:
        if not texts:
            return np.zeros((0, 0), dtype="float32")

        if self.provider == "openai":
            vecs = []
            B = 128
            for i in range(0, len(texts), B):
                chunk = texts[i:i+B]
                resp = self.client.embeddings.create(
                    model=self.model_name,
                    input=chunk,
                )
                vecs.extend([d.embedding for d in resp.data])

            X = np.array(vecs, dtype="float32")
            X = self._l2_normalize(X)
            return X.astype("float32")

        # provider = local (SentenceTransformer)
        X = self.model.encode(
            texts,
            convert_to_numpy=True,
            normalize_embeddings=True,
        )
        return X.astype("float32")
