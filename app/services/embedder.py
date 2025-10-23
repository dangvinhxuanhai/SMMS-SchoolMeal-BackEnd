from sentence_transformers import SentenceTransformer
import numpy as np

class Embedder:
    def __init__(self, model_name: str):
        self.model = SentenceTransformer(model_name)
    def encode(self, texts: list[str]) -> np.ndarray:
        X = self.model.encode(texts, convert_to_numpy=True, normalize_embeddings=True)
        return X.astype("float32")
