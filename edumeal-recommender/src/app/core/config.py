import os
from pydantic import BaseModel, Field
from dotenv import load_dotenv
load_dotenv()

class Settings(BaseModel):
    database_url: str = Field(default_factory=lambda: os.getenv("DATABASE_URL", ""))
    embed_model: str = Field(default_factory=lambda: os.getenv("EMBED_MODEL", "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"))
    faiss_index_path: str = Field(default_factory=lambda: os.getenv("FAISS_INDEX_PATH", "./data/recipes.index"))
    recipes_text_path: str = Field(default_factory=lambda: os.getenv("RECIPES_TEXT_PATH", "./data/recipes_text.csv"))
    top_k_search: int = int(os.getenv("TOP_K_SEARCH", "120"))
    use_pgvector: bool = os.getenv("USE_PGVECTOR","false").lower()=="true"
    save_embeddings: bool = os.getenv("SAVE_EMBEDDINGS","false").lower()=="true"

settings = Settings()
