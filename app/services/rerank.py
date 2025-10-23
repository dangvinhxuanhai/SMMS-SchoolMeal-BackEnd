import numpy as np, pandas as pd, joblib, math

def score_linear(feat: pd.DataFrame) -> pd.Series:
    # baseline trọng số tay khi CHƯA có model ML
    w = dict(faiss_sim=1.5, cov_main=1.2, ppr=0.8, risk=-2.0, cal_pen=-0.7, equip_ok=0.5)
    s = sum(w[k]*feat[k] for k in w if k in feat)
    return s

def mmr_select(ids_sorted, id2vec, id2score, top_n=5, lam=0.7):
    import numpy as np

    def _to_int(x):
        try: return int(x)
        except Exception: return x

    ids_sorted = [_to_int(x) for x in ids_sorted]
    id2vec     = {_to_int(k): v for k, v in id2vec.items()}
    id2score   = {_to_int(k): v for k, v in id2score.items()}

    # chỉ giữ ID có đủ vec + score
    ids_sorted = [i for i in ids_sorted if i in id2vec and i in id2score]
    if not ids_sorted:
        return []

    chosen = [ids_sorted[0]]
    pool = set(ids_sorted[1:])
    K = min(top_n, len(ids_sorted))

    while len(chosen) < K and pool:
        best = None
        best_val = -1e18
        for rid in list(pool):
            if rid not in id2vec or rid not in id2score:
                pool.discard(rid); continue
            rel = float(id2score[rid])
            div = max(float(np.dot(id2vec[rid], id2vec[c]))
                      for c in chosen if c in id2vec) if chosen else 0.0
            val = lam*rel - (1.0-lam)*div
            if val > best_val:
                best_val, best = val, rid
        if best is None: break
        chosen.append(best)
        pool.discard(best)
    return chosen


class MLRanker:
    def __init__(self, pkl_path: str|None):
        self.model = joblib.load(pkl_path) if pkl_path and os.path.exists(pkl_path) else None

    def predict(self, feat: pd.DataFrame) -> pd.Series:
        if self.model is None:
            return score_linear(feat)
        cols = self.model.feature_name_  # ensure đúng thứ tự cột khi train
        return pd.Series(self.model.predict(feat[cols]), index=feat.index)
