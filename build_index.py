import pandas as pd
import numpy as np
from sentence_transformers import SentenceTransformer
import faiss

# Load data
df = pd.read_csv("data/recipes.csv")

# Chuẩn bị text
df["doc_text"] = df.apply(lambda row: f"{row['Name']}. {row['Description']} "
                                      f"Calories: {row['Calories']}, Protein: {row['Protein']}g, "
                                      f"Fat: {row['Fat']}g, Carbs: {row['Carbs']}g. "
                                      f"Allergens: {row['Allergens']}. "
                                      f"Ingredients: {row['IngredientsJson']}", axis=1)

# Load model
model = SentenceTransformer("paraphrase-multilingual-MiniLM-L12-v2")

# Tạo embeddings
embeddings = model.encode(df["doc_text"].tolist(), convert_to_numpy=True)

# Chuẩn hóa (cosine sim)
emb_norm = embeddings / np.linalg.norm(embeddings, axis=1, keepdims=True)

# FAISS index
d = emb_norm.shape[1]
index = faiss.IndexFlatIP(d)
index.add(emb_norm)

# Save
df.to_csv("data/recipes_with_text.csv", index=False)
np.save("data/recipe_embeddings.npy", embeddings)
faiss.write_index(index, "data/recipes.index")

print("✅ Done: created embeddings + FAISS index")
