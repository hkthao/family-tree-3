import os

class Settings:
    REPLICATE_API_TOKEN: str = os.getenv("REPLICATE_API_TOKEN", "")
    # Add other settings here if needed

settings = Settings()

if not settings.REPLICATE_API_TOKEN:
    print("WARNING: REPLICATE_API_TOKEN environment variable is not set. Replicate API calls will fail.")
