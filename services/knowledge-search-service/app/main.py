from fastapi import FastAPI, Response, status
from fastapi.responses import ORJSONResponse
from loguru import logger

from .api import search, vectors
from .core.lancedb import lancedb_service
from .core.embeddings import embedding_service # Import the instance directly

app = FastAPI(
    title="Knowledge Search Service",
    description="Service for scoped vector search in LanceDB for RAG pipelines.",
    default_response_class=ORJSONResponse
)

# Include API routers
app.include_router(search.router, prefix="/api/v1/search", tags=["Search"])
app.include_router(vectors.router, prefix="/api/v1/vectors", tags=["Vectors"])

@app.on_event("startup")
async def startup_event():
    logger.info("Starting up Knowledge Search Service...")
    # Initialize embedding service to load model
    _ = embedding_service # Access the already imported global instance
    # Create a dummy LanceDB table for testing/demonstration
    # In a real scenario, this would be handled by data ingestion
    lancedb_service.create_dummy_table("F123") # Example family_id
    logger.info("Knowledge Search Service started.")

@app.get("/health", status_code=status.HTTP_200_OK)
async def health_check():
    """
    Health check endpoint to ensure the service is running.
    """
    logger.info("Health check requested.")
    return {"status": "ok"}

