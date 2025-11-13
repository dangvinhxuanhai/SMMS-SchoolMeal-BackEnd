# -*- coding: utf-8 -*-
# build_index.py (tên gợi ý)
# Mục đích script (chạy OFFLINE để chuẩn bị dữ liệu cho API):
# - Đọc danh sách recipes từ CSV.
# - Sinh văn bản mô tả (doc_text) từ các trường (tên, mô tả, nguyên liệu, allergen, dinh dưỡng, season...).
# - Embed doc_text bằng:
#       + local SentenceTransformer (mặc định) HOẶC
#       + OpenAI Embeddings (nếu bật --use_openai).
# - Chuẩn hoá vector (L2-normalize) để dùng Inner Product ≈ cosine similarity.
# - Xây FAISS index (Flat / IVF / HNSW) từ embeddings.
# - Lưu:
#       + embeddings_npy: ma trận embedding.
#       + recipes_with_text.csv: CSV đã có doc_text (phục vụ graph, debug).
#       + recipes.index: FAISS index.
#       + metadata.json: thông tin cấu hình để API load cho đúng.

import os, json, argparse, math, time
import numpy as np
import pandas as pd
import faiss

# --------------------------
# Helpers
# --------------------------
def parse_list(x):
    """
    Chuẩn hóa 1 ô thành list[str].

    Hỗ trợ:
    - None / NaN
    - list đã chuẩn
    - chuỗi JSON list: ["a", "b"]
    - chuỗi Python list: ['a', 'b']
    - chuỗi "a, b, c"
    """
    if x is None or (isinstance(x, float) and math.isnan(x)):
        return []

    # Nếu đã là list -> strip từng phần tử
    if isinstance(x, list):
        return [str(i).strip() for i in x if str(i).strip()]

    s = str(x).strip()
    if not s:
        return []

    # 1) Thử parse JSON list
    if s.startswith("[") and s.endswith("]"):
        # Có thể là JSON hoặc Python literal
        try:
            j = json.loads(s)
            if isinstance(j, list):
                return [str(i).strip() for i in j if str(i).strip()]
        except Exception:
            try:
                j = ast.literal_eval(s)  # xử lý kiểu ['a', 'b']
                if isinstance(j, (list, tuple)):
                    return [str(i).strip() for i in j if str(i).strip()]
            except Exception:
                pass

    # 2) Fallback: tách theo dấu phẩy, bỏ ' " [ ]
    items = []
    for p in s.split(","):
        t = p.strip().strip("'").strip('"').strip()
        if t and t not in ("[", "]"):
            items.append(t)
    return items


def make_doc_text(row):
    """
    Ghép các trường trong một dòng recipe thành chuỗi doc_text cho embedding.

    Có thể tuỳ chỉnh theo schema thực tế, nhưng ở đây:
    - Name, Description
    - Ingredients (list)
    - Allergens (list)
    - Nutrition: Calories, Protein, Fat, Carbs
    - Cuisine, Season

    Mục tiêu:
        doc_text chứa đủ ngữ cảnh semantic để FAISS tìm kiếm theo ý đồ.
    """
    parts = []

    # Tên & mô tả món
    for col in ["Name", "Description"]:
        if col in row and str(row[col]).strip():
            parts.append(str(row[col]))

    # Nguyên liệu
    if "Ingredients" in row:
        ings = parse_list(row["Ingredients"])
        if ings:
            parts.append("Ingredients: " + ", ".join(ings))

    # Allergen
    if "Allergens" in row:
        allergens = parse_list(row["Allergens"])
        if allergens:
            parts.append("Allergens: " + ", ".join(allergens))

    # Dinh dưỡng cơ bản
    for col in ["Calories", "Protein", "Fat", "Carbs"]:
        if col in row and str(row[col]).strip():
            parts.append(f"{col}: {row[col]}")

    # Ẩm thực & mùa
    if "Cuisine" in row and str(row["Cuisine"]).strip():
        parts.append("Cuisine: " + str(row["Cuisine"]))
    if "Season" in row and str(row["Season"]).strip():
        parts.append("Season: " + str(row["Season"]))

    # Ghép lại thành một chuỗi, phân tách bằng " | "
    return " | ".join(parts)


def l2_normalize(X: np.ndarray, eps=1e-12) -> np.ndarray:
    """
    Chuẩn hoá L2 theo hàng:
    - X: (N, d)
    - Trả: X_norm sao cho mỗi vector có norm ≈ 1.
    Dùng để:
      - Inner Product ≈ Cosine similarity.
    """
    n = np.linalg.norm(X, axis=1, keepdims=True)
    return X / (n + eps)


def embed_local(texts, model_name: str, batch_size: int = 64) -> np.ndarray:
    """
    Sinh embedding bằng SentenceTransformer (chạy local, không cần API key).
    - normalize_embeddings=True: vector đã L2-normalize.
    - Trả về float32 để giảm dung lượng & tương thích FAISS.
    """
    from sentence_transformers import SentenceTransformer
    model = SentenceTransformer(model_name)
    X = model.encode(
        texts,
        convert_to_numpy=True,
        normalize_embeddings=True,
        batch_size=batch_size,
    )
    return X.astype("float32")


def embed_openai(texts, openai_model: str) -> np.ndarray:
    """
    Sinh embedding bằng OpenAI Embeddings API.
    - Yêu cầu biến môi trường OPENAI_API_KEY.
    - Chunk theo B=128 để tránh giới hạn.
    - Chuẩn hoá L2 sau khi nhận về.
    """
    from openai import OpenAI
    client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

    vecs = []
    B = 128  # batch size khi gọi API
    for i in range(0, len(texts), B):
        chunk = texts[i : i + B]
        resp = client.embeddings.create(model=openai_model, input=chunk)
        vecs.extend([d.embedding for d in resp.data])

    X = np.array(vecs, dtype="float32")
    X = l2_normalize(X)
    return X


def build_index_flat(X: np.ndarray):
    """
    Xây FAISS IndexFlatIP:
    - Dùng Inner Product (IP).
    - Với vector đã normalize, IP = cosine similarity.
    - Phù hợp dataset nhỏ/vừa, chính xác 100%.
    """
    d = X.shape[1]
    index = faiss.IndexFlatIP(d)
    index.add(X)
    return index


def build_index_ivf(X: np.ndarray, nlist=1024, nprobe=16):
    """
    Xây FAISS IVF Flat Index:
    - nlist: số centroid (số cell phân bố không gian).
    - nprobe: số cell duyệt khi search (trade-off speed/accuracy).
    - Phù hợp dataset lớn hơn.
    """
    d = X.shape[1]
    quantizer = faiss.IndexFlatIP(d)  # dùng IP cho coarse quantizer
    index = faiss.IndexIVFFlat(quantizer, d, nlist, faiss.METRIC_INNER_PRODUCT)
    index.train(X)        # BẮT BUỘC phải train trước khi add
    index.add(X)
    index.nprobe = int(nprobe)
    return index


def build_index_hnsw(X: np.ndarray, M=32, efC=200, efS=64):
    """
    Xây FAISS HNSW Index:
    - M   : số neighbor cho mỗi node trong HNSW graph.
    - efC : efConstruction, ảnh hưởng chất lượng graph.
    - efS : efSearch, trade-off speed/accuracy khi query.

    Ở đây dùng IndexHNSWFlat với metric mặc định L2.
    Vì X đã normalize, L2 distance ~ cosine, có thể sử dụng được.
    """
    d = X.shape[1]
    index = faiss.IndexHNSWFlat(d, M)

    # Thiết lập tham số HNSW qua ParameterSpace
    ps = faiss.ParameterSpace()
    ps.set_index_parameter(index, "efConstruction", int(efC))

    index.add(X)

    ps.set_index_parameter(index, "efSearch", int(efS))
    return index


# --------------------------
# Main
# --------------------------
def main():
    # Khai báo arguments CLI để chạy linh hoạt:
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes.csv")                 # input gốc
    ap.add_argument("--out_csv", default="data/recipes_with_text.csv")          # output có doc_text
    ap.add_argument("--embeddings_npy", default="index/recipe_embeddings.npy")  # nơi lưu embeddings
    ap.add_argument("--index_out", default="index/recipes.index")               # nơi lưu FAISS index
    ap.add_argument("--metadata_out", default="index/metadata.json")            # nơi lưu metadata
    ap.add_argument("--model", default="paraphrase-multilingual-MiniLM-L12-v2") # model local
    ap.add_argument("--use_openai", action="store_true")                        # bật dùng OpenAI
    ap.add_argument("--openai_model", default="text-embedding-3-small")         # model openai
    ap.add_argument("--index_type", choices=["flat", "ivf", "hnsw"], default="flat")
    ap.add_argument("--nlist", type=int, default=1024)
    ap.add_argument("--nprobe", type=int, default=16)
    ap.add_argument("--M", type=int, default=32)
    ap.add_argument("--efC", type=int, default=200)
    ap.add_argument("--efS", type=int, default=64)
    ap.add_argument("--batch_size", type=int, default=64)
    args = ap.parse_args()

    # Đảm bảo các thư mục output tồn tại
    os.makedirs(os.path.dirname(args.out_csv), exist_ok=True)
    os.makedirs(os.path.dirname(args.embeddings_npy), exist_ok=True)
    os.makedirs(os.path.dirname(args.index_out), exist_ok=True)

    print(f"[build_index] Load: {args.recipes_csv}")
    df = pd.read_csv(args.recipes_csv)

    # ----------------- Chuẩn hoá cột cơ bản -----------------
    # Nếu không có RecipeId -> dùng index làm RecipeId
    if "RecipeId" not in df.columns:
        df["RecipeId"] = df.index.astype(str)

    # Đảm bảo có Name (fallback = RecipeId)
    df["Name"] = df.get("Name", df["RecipeId"]).astype(str)

    # Chuẩn hoá numeric cho cột dinh dưỡng
    for col in ["Calories", "Protein", "Fat", "Carbs"]:
        if col in df.columns:
            df[col] = (
                pd.to_numeric(df[col], errors="coerce")
                .fillna(0)
                .astype(float)
            )

    # Chuẩn hoá list-like cho Ingredients / Allergens / Equipment
    for col in ["Ingredients", "Allergens", "Equipment"]:
        if col in df.columns:
            df[col] = df[col].apply(parse_list)
        else:
            df[col] = [[] for _ in range(len(df))]

    # Nếu chưa có DishType, mặc định là "Main"
    if "DishType" not in df.columns:
        df["DishType"] = ["Main"] * len(df)

    # ----------------- Tạo doc_text -----------------
    print("[build_index] Build doc_text ...")
    df["doc_text"] = df.apply(make_doc_text, axis=1)

    # ----------------- Sinh Embeddings -----------------
    print(f"[build_index] Embedding ({'OpenAI' if args.use_openai else 'local'}) ...")
    texts = df["doc_text"].tolist()

    if args.use_openai:
        # Dùng OpenAI Embeddings
        X = embed_openai(texts, args.openai_model)
        emb_meta = {"provider": "openai", "model": args.openai_model}
    else:
        # Dùng SentenceTransformer local
        X = embed_local(texts, args.model, batch_size=args.batch_size)
        emb_meta = {"provider": "sbert", "model": args.model}

    # Lưu embeddings ra .npy để API/retriever dùng lại
    np.save(args.embeddings_npy, X)

    N = X.shape[0]
    print(f"[build_index] rows={N}, dim={X.shape[1]}")

    # ----------------- (Tuỳ chọn) Cảnh báo / điều chỉnh IVF -----------------
    if args.index_type == "ivf":
        # Điều chỉnh nlist/nprobe hợp lý với N (tránh > N)
        eff_nlist = max(1, min(args.nlist, N))
        eff_nprobe = max(1, min(args.nprobe, eff_nlist))

        if eff_nlist != args.nlist:
            print(f"[warn] nlist={args.nlist} > N={N} ⇒ dùng nlist={eff_nlist}")
        if eff_nprobe != args.nprobe:
            print(
                f"[warn] nprobe={args.nprobe} > nlist={eff_nlist} ⇒ dùng nprobe={eff_nprobe}"
            )

        # Gợi ý: nếu N nhỏ thì IVF thường không lợi
        if N < 1000:
            print(
                "[warn] N nhỏ (<1000): IVF thường không lợi; cân nhắc dùng --index_type flat"
            )

        # (Lưu ý: đoạn này build index 1 lần với eff_nlist/eff_nprobe.
        #  Trong code hiện tại phía dưới còn block build lại index theo args gốc;
        #  bạn có thể refactor để chỉ giữ 1 nơi build cho rõ ràng.)
        index = build_index_ivf(X, nlist=eff_nlist, nprobe=eff_nprobe)

    # ----------------- Build index chính -----------------
    print(f"[build_index] Build index type={args.index_type} ...")
    if args.index_type == "flat":
        index = build_index_flat(X)
    elif args.index_type == "ivf":
        # Ở đây đang build lại với args.nlist/nprobe gốc.
        # Nếu muốn dùng eff_nlist/eff_nprobe ở trên, chỉnh lại cho thống nhất.
        index = build_index_ivf(X, nlist=args.nlist, nprobe=args.nprobe)
    else:  # hnsw
        index = build_index_hnsw(
            X,
            M=args.M,
            efC=args.efC,
            efS=args.efS,
        )

    # ----------------- Lưu index & CSV -----------------
    faiss.write_index(index, args.index_out)   # Lưu FAISS index
    df.to_csv(args.out_csv, index=False)       # Lưu CSV đã có doc_text (dùng cho graph, debug)

    # ----------------- Lưu metadata -----------------
    meta = {
        "built_at": time.strftime("%Y-%m-%d %H:%M:%S"),
        "index_type": args.index_type,
        "index_out": args.index_out,
        "recipes_csv": args.recipes_csv,
        "rows": int(len(df)),
        "dim": int(X.shape[1]),
        "embedding": emb_meta,
        "ivf": {
            "nlist": args.nlist,
            "nprobe": args.nprobe,
        } if args.index_type == "ivf" else None,
        "hnsw": {
            "M": args.M,
            "efC": args.efC,
            "efS": args.efS,
        } if args.index_type == "hnsw" else None,
    }

    with open(args.metadata_out, "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)

    print("[build_index] Done.")
    print(json.dumps(meta, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    # Cho phép chạy:
    #   python build_index.py --recipes_csv ... --index_type flat/ivf/hnsw ...
    main()
