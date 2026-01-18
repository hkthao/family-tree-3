from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    OLLAMA_BASE_URL: str = "http://localhost:11434"
    OPENAI_API_KEY: str = "sk-xxx" # Placeholder, user should set this
    DEFAULT_TEMPERATURE: float = 0.0
    DEFAULT_MAX_TOKENS: int = 512

    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

settings = Settings()
