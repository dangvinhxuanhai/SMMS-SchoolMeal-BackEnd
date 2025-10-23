import faiss, numpy as np, pandas as pd

class FaissRetriever:
    def __init__(self, index_path: str, df: pd.DataFrame):
        self.index = faiss.read_index(index_path)
        self.df = df

    def search(self, qv: np.ndarray, k: int) -> pd.DataFrame:
        D, I = self.index.search(qv, k)
        c = self.df.iloc[I[0]].copy()
        c["faiss_sim"] = D[0]
        return c
