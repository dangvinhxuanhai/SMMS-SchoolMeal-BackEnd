# scripts/build_kitchen_graph.py (tên gợi ý)
# Mục đích:
# - Đọc dữ liệu công thức (recipes) từ file CSV.
# - Chuẩn hoá các cột Ingredients / Allergens / Equipment / Season.
# - Xây dựng đồ thị kiến thức (NetworkX DiGraph) cho domain Kitchen với các node & cạnh:
#     Nodes:
#       ("Recipe", id)
#       ("Ingredient", name)
#       ("Allergen", name)
#       ("Equipment", name)
#       ("Season", name)
#     Edges:
#       Recipe --USES--> Ingredient
#       Recipe --HAS_ALLERGEN--> Allergen
#       Recipe --REQUIRES--> Equipment
#       Recipe --SUITS_SEASON--> Season
# - Lưu đồ thị ra file .gpickle (tương thích NetworkX 2.x/3.x) để service sử dụng (KitchenGraph).

import os, argparse, json, math
import pandas as pd
import networkx as nx

# Hỗ trợ lưu gpickle tương thích cả NetworkX 2.x/3.x:
# - Nếu networkx.readwrite.gpickle tồn tại (NX 3.x) thì dùng trực tiếp.
# - Nếu không, fallback dùng pickle thủ công.
try:
    from networkx.readwrite import gpickle as nx_gpickle  # NetworkX 3.x style
except Exception:
    nx_gpickle = None
    import pickle  # Fallback cho môi trường không có readwrite.gpickle


def save_gpickle(G, path: str):
    """
    Lưu graph G ra file path dạng .gpickle.
    - Tự tạo thư mục cha nếu chưa tồn tại.
    - Ưu tiên dùng networkx_gpickle.write_gpickle, nếu không có thì dùng pickle.dump.
    """
    os.makedirs(os.path.dirname(path), exist_ok=True)
    if nx_gpickle is not None:
        # Cách chính thức (NetworkX hỗ trợ)
        nx_gpickle.write_gpickle(G, path)
    else:
        # Fallback: dùng pickle thuần tuý
        with open(path, "wb") as f:
            pickle.dump(G, f)



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


def main():
    # Khai báo argument CLI:
    # --recipes_csv: đường dẫn CSV input
    # --out_graph  : nơi lưu graph .gpickle
    ap = argparse.ArgumentParser()
    ap.add_argument("--recipes_csv", default="data/recipes_with_text.csv")
    ap.add_argument("--out_graph",   default="graph/kitchen_graph.gpickle")
    args = ap.parse_args()

    # Đảm bảo thư mục output tồn tại
    os.makedirs(os.path.dirname(args.out_graph), exist_ok=True)

    # Đọc dữ liệu công thức
    df = pd.read_csv(args.recipes_csv)

    # Nếu không có cột RecipeId thì dùng index làm RecipeId (string)
    if "RecipeId" not in df.columns:
        df["RecipeId"] = df.index.astype(str)

    # Chuẩn hoá các cột dạng danh sách:
    # Nếu cột không tồn tại, tạo cột rỗng tương ứng (list trống cho mỗi dòng)
    for col in ["Ingredients", "Allergens", "Equipment"]:
        if col in df.columns:
            df[col] = df[col].apply(parse_list)
        else:
            df[col] = [[] for _ in range(len(df))]

    # Xác định cột Season nếu có
    season_col = "Season" if "Season" in df.columns else None

    # Tạo đồ thị có hướng
    G = nx.DiGraph()

    # Duyệt từng dòng recipe để tạo node + cạnh
    for r in df.itertuples():
        # Lấy RecipeId
        rid = str(getattr(r, "RecipeId"))
        # Node món ăn: ("Recipe", id)
        rnode = ("Recipe", rid)
        # Tên món (fallback dùng rid nếu thiếu)
        name = str(getattr(r, "Name", rid))
        # Loại món (Main/Side/...) nếu có
        dish_type = str(getattr(r, "DishType", ""))

        # Thêm node Recipe với metadata
        G.add_node(rnode, Name=name, DishType=dish_type)

        # ----- Ingredients -----
        for ing in getattr(r, "Ingredients", []):
            # Node nguyên liệu: ("Ingredient", tên thường, lowercase)
            inode = ("Ingredient", ing.lower())
            G.add_node(inode)
            # Cạnh: Recipe --USES--> Ingredient
            G.add_edge(rnode, inode, type="USES")
            # Ingredient -> Recipe (thêm cạnh ngược, để PPR từ seed Ingredient đi ngược về Recipe)
            G.add_edge(inode, rnode, type="USED_IN")

        # ----- Allergens -----
        for alg in getattr(r, "Allergens", []):
            anode = ("Allergen", alg.lower())
            G.add_node(anode)
            # Cạnh: Recipe --HAS_ALLERGEN--> Allergen
            G.add_edge(rnode, anode, type="HAS_ALLERGEN")

        # ----- Equipment -----
        for eq in getattr(r, "Equipment", []):
            enode = ("Equipment", eq.lower())
            G.add_node(enode)
            # Cạnh: Recipe --REQUIRES--> Equipment
            G.add_edge(rnode, enode, type="REQUIRES")

        # ----- Season -----
        if season_col:
            s = str(getattr(r, season_col)).strip()
            if s:
                snode = ("Season", s.lower())
                G.add_node(snode)
                # Cạnh: Recipe --SUITS_SEASON--> Season
                G.add_edge(rnode, snode, type="SUITS_SEASON")

    # Lưu graph ra file .gpickle
    save_gpickle(G, args.out_graph)

    # In thông tin tóm tắt để log / debug
    print(f"[build_graph] Nodes: {G.number_of_nodes()}, Edges: {G.number_of_edges()}")
    print(f"[build_graph] Saved to: {args.out_graph}")


if __name__ == "__main__":
    # Cho phép chạy script trực tiếp:
    # python build_kitchen_graph.py --recipes_csv ... --out_graph ...
    main()
