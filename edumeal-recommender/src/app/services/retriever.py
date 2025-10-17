import faiss
import numpy as np
import pandas as pd
from sentence_transformers import SentenceTransformer
import networkx as nx

class FaissRetriever:
    def __init__(self, index_path: str, metadata_path: str, model_name: str = "all-MiniLM-L6-v2"):
        self.index = faiss.read_index(index_path)
        self.metadata = pd.read_parquet(metadata_path)
        self.model = SentenceTransformer(model_name)

        # Map rowid <-> food_id
        self.rowid_to_foodid = dict(enumerate(self.metadata["food_id"]))
        self.foodid_to_rowid = {v: k for k, v in self.rowid_to_foodid.items()}

    def _encode_query(self, qtxt: str) -> np.ndarray:
        emb = self.model.encode([qtxt])
        return emb.astype("float32")

    def search(self, qtxt: str, k: int = 20, include_graph: bool = True):
        qvec = self._encode_query(qtxt)
        sims, rowids = self.index.search(qvec, k)  # (1, k)

        sims = sims[0]
        rowids = rowids[0]

        # Build result dataframe
        df = self.metadata.iloc[rowids].copy()
        df["faiss_rowid"] = rowids
        df["semantic_sim"] = sims

        if include_graph:
            df["graph_rank_score"] = self._compute_graph_scores(df)

        df = df.sort_values("semantic_sim", ascending=False).reset_index(drop=True)
        return df, rowids, sims

    def _compute_graph_scores(self, df: pd.DataFrame) -> pd.Series:
        """
        Optional: Build food similarity graph from top-k results
        and use centrality as a reranking signal.
        """
        G = nx.Graph()

        # Add nodes with semantic sim
        for _, row in df.iterrows():
            G.add_node(row["food_id"], weight=row["semantic_sim"])

        # Add edges based on similarity of descriptions
        descs = df["food_name"].astype(str).tolist()
        emb_matrix = self.model.encode(descs).astype("float32")

        for i in range(len(descs)):
            for j in range(i + 1, len(descs)):
                sim = float(np.dot(emb_matrix[i], emb_matrix[j]) /
                            (np.linalg.norm(emb_matrix[i]) * np.linalg.norm(emb_matrix[j]) + 1e-8))
                if sim > 0.7:  # threshold để kết nối
                    G.add_edge(df.iloc[i]["food_id"], df.iloc[j]["food_id"], weight=sim)

        # Centrality như một signal cho độ quan trọng trong cluster
        centrality = nx.pagerank(G, alpha=0.85, weight="weight")

        return df["food_id"].map(centrality).fillna(0)
