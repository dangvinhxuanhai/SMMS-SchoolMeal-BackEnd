# app/services/retrieval.py
import faiss, numpy as np, pandas as pd

class FaissRetriever:
    def __init__(self, index_path: str, df: pd.DataFrame):
        self.index = faiss.read_index(index_path)
        self.df = df

    def search(self, qv: np.ndarray, k: int) -> pd.DataFrame:
        D, I = self.index.search(qv, k)  # I[0]: các vị trí hàng trong self.df
        idx = I[0]
        sim = D[0]

        # Khử trùng lặp theo vị trí embedding (giữ lần đầu)
        mask_unique = ~pd.Series(idx).duplicated(keep="first").to_numpy()
        idx_u = idx[mask_unique]
        sim_u = sim[mask_unique]

        # Lấy ứng viên theo vị trí đã khử trùng lặp
        cands = self.df.iloc[idx_u].copy()
        cands["faiss_sim"] = sim_u
        # (tuỳ chọn) đảm bảo index là int để các bước sau dễ xử lý
        try:
            cands.index = cands.index.astype(int)
        except Exception:
            pass

        return cands

