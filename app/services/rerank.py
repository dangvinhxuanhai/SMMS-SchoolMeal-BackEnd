# app/services/rerank.py
# Chức năng chính:
# - score_linear: baseline scoring dùng trọng số tay khi CHƯA (hoặc KHÔNG) có model học máy.
# - mmr_select: áp dụng thuật toán Maximal Marginal Relevance (MMR)
#               để chọn danh sách kết quả vừa "tốt" vừa "đa dạng".
# - MLRanker: wrapper cho model ML (joblib), tự động fallback sang score_linear nếu không có model.
#
# File này là "tầng rerank cuối" trong pipeline:
# FAISS (ứng viên gần) → build_features → MLRanker/score_linear → mmr_select → danh sách món trả về.

import os
import numpy as np
import pandas as pd
import joblib
import math


def score_linear(feat: pd.DataFrame) -> pd.Series:
    """
    Hàm baseline tính điểm tuyến tính khi chưa dùng model ML thực thụ.

    feat: DataFrame features cho từng ứng viên, ví dụ các cột:
        - faiss_sim : độ giống semantic từ FAISS
        - cov_main  : độ phủ nguyên liệu chính
        - ppr       : score từ Personalized PageRank
        - risk      : điểm rủi ro dị ứng (càng cao càng xấu)
        - cal_pen   : penalty calo (vượt chuẩn)
        - equip_ok  : có phù hợp thiết bị hay không

    Trọng số w được gán tay (domain knowledge):
        - faiss_sim: 1.5   (ưu tiên gần semantic)
        - cov_main : 1.2   (ưu tiên khớp nguyên liệu)
        - ppr      : 0.8   (ưu tiên gần trên graph)
        - risk     : -2.0  (phạt mạnh món rủi ro dị ứng)
        - cal_pen  : -0.7  (phạt món vượt calo)
        - equip_ok : 0.5   (thưởng món phù hợp thiết bị)

    Trả về:
        - pd.Series điểm cho từng dòng trong feat.
    """
    # Trọng số baseline, có thể tinh chỉnh dần theo thực nghiệm
    w = dict(
        faiss_sim=1.5,
        cov_main=1.2,
        ppr=0.8,
        risk=-2.0,
        cal_pen=-0.7,
        equip_ok=0.5,
    )

    # Tính tổng có trọng số cho các feature tồn tại trong feat
    # sum(...) tạo thành một Series vì mỗi vế là Series
    s = sum(w[k] * feat[k] for k in w if k in feat)
    return s


def mmr_select(ids_sorted, id2vec, id2score, top_n=5, lam=0.7):
    """
    Maximal Marginal Relevance (MMR):

    Mục tiêu:
      - Chọn ra tối đa top_n ID từ danh sách ids_sorted (đã sort theo score giảm dần),
      - sao cho:
          + relevance cao (id2score)
          + nhưng vẫn đa dạng, ít trùng lặp thông tin (dựa trên cosine similarity giữa vector id2vec).

    Tham số:
      - ids_sorted: list ID ứng viên, đã sort theo score (cao -> thấp).
      - id2vec: dict {id -> embedding vector}.
      - id2score: dict {id -> score}.
      - top_n: số phần tử muốn chọn.
      - lam: hệ số [0,1], trade-off:
          + lam gần 1: ưu tiên relevance.
          + lam nhỏ: ưu tiên diversity.

    Trả về:
      - List ID đã chọn theo thứ tự MMR.
    """

    def _to_int(x):
        # Helper: cố gắng ép key về int để đồng nhất kiểu; nếu không được thì giữ nguyên.
        try:
            return int(x)
        except Exception:
            return x

    # Chuẩn hóa ID về cùng kiểu (tránh lệch key giữa ids_sorted, id2vec, id2score)
    ids_sorted = [_to_int(x) for x in ids_sorted]
    id2vec = {_to_int(k): v for k, v in id2vec.items()}
    id2score = {_to_int(k): v for k, v in id2score.items()}

    # Chỉ giữ lại những ID có đủ cả vector lẫn score
    ids_sorted = [i for i in ids_sorted if i in id2vec and i in id2score]
    if not ids_sorted:
        return []

    # Khởi tạo: chọn phần tử tốt nhất đầu tiên (relevance cao nhất)
    chosen = [ids_sorted[0]]
    # pool: các ứng viên còn lại
    pool = set(ids_sorted[1:])
    # K là số lượng thực tế sẽ chọn (không vượt quá số ứng viên)
    K = min(top_n, len(ids_sorted))

    # Lặp cho đến khi đủ K hoặc hết ứng viên
    while len(chosen) < K and pool:
        best = None
        best_val = -1e18  # Giá trị rất nhỏ để bắt max

        # Duyệt từng ứng viên còn lại
        for rid in list(pool):
            # Bỏ qua nếu thiếu vec hoặc score (phòng thủ)
            if rid not in id2vec or rid not in id2score:
                pool.discard(rid)
                continue

            # relevance: điểm chất lượng riêng lẻ
            rel = float(id2score[rid])

            # diversity penalty: độ giống tối đa với bất kỳ phần tử đã chọn
            # (dùng dot product vì vec đã normalize -> cosine similarity)
            div = (
                max(
                    float(np.dot(id2vec[rid], id2vec[c]))
                    for c in chosen
                    if c in id2vec
                )
                if chosen
                else 0.0
            )

            # Hàm mục tiêu MMR:
            # lam * relevance - (1 - lam) * similarity
            val = lam * rel - (1.0 - lam) * div

            # Chọn ứng viên có val lớn nhất
            if val > best_val:
                best_val, best = val, rid

        if best is None:
            break

        # Thêm vào danh sách chọn và loại khỏi pool
        chosen.append(best)
        pool.discard(best)

    return chosen


class MLRanker:
    """
    Wrapper cho model ML dùng để rerank:

    - Nếu cung cấp pkl_path hợp lệ:
        + Load model (vd: GradientBoosting, XGBoost, RandomForest...)
        + predict() dùng model để tính score.
    - Nếu không có model (model=None hoặc file không tồn tại):
        + Fallback dùng score_linear(feat).

    Giúp code phía trên luôn gọi RANKER.predict() mà không cần if/else rải rác.
    """

    def __init__(self, pkl_path: str | None):
        # Nếu có đường dẫn và file tồn tại -> load model
        # Nếu không -> self.model = None, sẽ fallback score_linear
        self.model = (
            joblib.load(pkl_path)
            if pkl_path and os.path.exists(pkl_path)
            else None
        )

    def predict(self, feat: pd.DataFrame) -> pd.Series:
        """
        Tính score cho từng dòng feature.

        - Nếu có model ML:
            + Lấy danh sách tên feature đã dùng lúc train: model.feature_name_
            + Đảm bảo chọn đúng cột + đúng thứ tự trước khi predict.
        - Nếu không có model:
            + Trả về score_linear(feat) làm baseline.
        """
        # Không có model -> dùng baseline score_linear
        if self.model is None:
            return score_linear(feat)

        # Đảm bảo thứ tự & tập feature khớp với lúc training
        cols = self.model.feature_name_  # attribute phổ biến của nhiều lib (xgboost/sklearn wrapper)
        # model.predict trả về ndarray, convert sang Series với index gốc
        return pd.Series(self.model.predict(feat[cols]), index=feat.index)
