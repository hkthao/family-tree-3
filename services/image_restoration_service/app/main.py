from fastapi import FastAPI
from app.api import router as api_router

app = FastAPI(
    title="Image Restoration Service",
    description="Service to restore old images using Replicate AI models (GFPGAN and Real-ESRGAN).",
    version="1.0.0",
)

app.include_router(api_router)

@app.get("/")
async def root():
    return {"message": "Image Restoration Service is running! Access API docs at /docs"}
