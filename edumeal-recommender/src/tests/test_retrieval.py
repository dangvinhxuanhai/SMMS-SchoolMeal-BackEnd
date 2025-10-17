from app.services.retriever import FaissRetriever

retriever = FaissRetriever(
    index_path="faiss.index",
    metadata_path="foods.parquet",
    model_name="all-MiniLM-L6-v2"
)

query = "Món ăn nhẹ cho trẻ bị dị ứng trứng"
results, _, _ = retriever.search(query, k=10)

print(results[["food_id", "food_name", "semantic_sim", "graph_rank_score"]])
