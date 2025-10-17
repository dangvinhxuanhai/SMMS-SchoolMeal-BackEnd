# settings.py
from functools import lru_cache
from pydantic_settings import BaseSettings, SettingsConfigDict
from pydantic import SecretStr, PositiveInt, field_validator

class AppSettings(BaseSettings):
    # ----- App meta -----
    APP_ENV: str = "dev"                      # dev|staging|prod
    DEBUG: bool = True

    # ----- OpenAI / LLM -----
    OPENAI_API_KEY: SecretStr | None = None   # dùng SecretStr để tránh lộ khi print
    LLM_MODEL: str = "gpt-4o-mini"
    LLM_TIMEOUT_SEC: float = 30

    # ----- RAG / Search -----
    FAISS_TOPK: PositiveInt = 15
    RESULT_TOPN: PositiveInt = 5

    # Nếu bạn có DB:
    DATABASE_URL: str = "sqlite:///./local.db"

    # Pydantic Settings config
    model_config = SettingsConfigDict(
        env_file=(".env", ".env.local"),      # đọc .env khi dev; file sau ghi đè file trước
        env_file_encoding="utf-8",
        case_sensitive=False,                  # ENV không phân biệt hoa-thường
        extra="ignore",                        # bỏ qua biến lạ
    )

    # Validate logic liên quan giữa các trường
    @field_validator("RESULT_TOPN")
    @classmethod
    def _topn_le_topk(cls, v, info):
        topk = info.data.get("FAISS_TOPK", 15)
        if v > topk:
            raise ValueError("RESULT_TOPN phải <= FAISS_TOPK")
        return v

# Factory có cache: tạo Settings 1 lần (rất quan trọng cho hiệu năng)
@lru_cache
def get_settings() -> AppSettings:
    return AppSettings()
