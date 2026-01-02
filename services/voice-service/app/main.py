from fastapi import FastAPI
from app.api.voice import router as voice_router
from loguru import logger

# Configure Loguru to write to stdout by default
logger.add(
    "file.log",
    rotation="500 MB",
    level="INFO",
    format="{time} {level} {message}",
    colorize=True,
    backtrace=True,
    diagnose=True
)
logger.info("Starting Voice AI Service application...")

app = FastAPI(
    title="Python Voice AI Service",
    description="A FastAPI service for voice preprocessing and AI-powered voice generation.",
    version="1.0.0",
)

app.include_router(voice_router)

@app.get("/")
async def root():
    return {"message": "Python Voice AI Service is running! Access API docs at /docs"}
