from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles
from app.api.voice import router as voice_router
from loguru import logger
import os
import asyncio
import time
from app.config import settings

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

# Mount static files directory
app.mount("/static", StaticFiles(directory=settings.STATIC_FILES_DIR), name="static")

app.include_router(voice_router)

@app.get("/")
async def root():
    return {"message": "Python Voice AI Service is running! Access API docs at /docs"}

async def cleanup_static_files():
    """
    Background task to periodically clean up old static files.
    Files older than STATIC_FILE_LIFETIME_SECONDS will be deleted.
    """
    while True:
        logger.info(f"Running static file cleanup. Deleting files older than {settings.STATIC_FILE_LIFETIME_SECONDS} seconds.")
        now = time.time()
        for filepath in settings.STATIC_FILES_DIR.iterdir():
            if filepath.is_file():
                file_age = now - filepath.stat().st_mtime
                if file_age > settings.STATIC_FILE_LIFETIME_SECONDS:
                    try:
                        os.remove(filepath)
                        logger.info(f"Deleted old static file: {filepath}")
                    except OSError as e:
                        logger.error(f"Error deleting old static file {filepath}: {e}")
        await asyncio.sleep(600) # Run cleanup every 10 minutes

@app.on_event("startup")
async def startup_event():
    """
    Starts the background cleanup task when the application starts.
    """
    asyncio.create_task(cleanup_static_files())
    logger.info("Static file cleanup background task started.")
