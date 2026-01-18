from contextlib import asynccontextmanager
from fastapi import FastAPI
from app.api import chat
from app.config import settings
import logging

# Configure logging
logging.basicConfig(level=logging.INFO,
                    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("LLM Gateway Service started.")
    logger.info(f"Ollama Base URL: {settings.OLLAMA_BASE_URL}")
    logger.info(f"OpenAI API Key (first 5 chars): {settings.OPENAI_API_KEY[:5]}...")
    yield
    # Ensure httpx client is closed on shutdown
    await chat.ollama_llm_client.client.aclose()
    logger.info("LLM Gateway Service shut down.")

app = FastAPI(
    title="LLM Gateway Service",
    description="A Python microservice acting as an OpenAI-style LLM Gateway for Ollama and OpenAI.",
    version="0.1.0",
    lifespan=lifespan # Use lifespan event handler
)

app.include_router(chat.router)

@app.get("/")
async def root():
    return {"message": "LLM Gateway Service is running. Access /docs for API documentation."}