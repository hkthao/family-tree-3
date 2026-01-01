import os

class Settings:
    REPLICATE_API_TOKEN: str = os.getenv("REPLICATE_API_TOKEN", "")
    BACKEND_API_URL: str = os.getenv("BACKEND_API_URL", "")
    # Add other settings here if needed

settings = Settings()

if not settings.REPLICATE_API_TOKEN:
    print("WARNING: REPLICATE_API_TOKEN environment variable is not set. Replicate API calls will fail.")
if not settings.BACKEND_API_URL:
    print("WARNING: BACKEND_API_URL environment variable is not set. Backend API calls will not be made.")
