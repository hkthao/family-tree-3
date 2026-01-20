from contextlib import asynccontextmanager

from fastapi import FastAPI
from app.api import faces, knowledge, search # Import the routers
from app.core.lancedb import initialize_lancedb_services
from app.core.embeddings import embedding_service as global_embedding_service

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: Initialize LanceDB services
    await initialize_lancedb_services(global_embedding_service)
    yield
    # Shutdown: No specific cleanup needed for LanceDB connection as it's handled internally

app = FastAPI(title="Knowledge Search Service API", lifespan=lifespan) # Updated title

@app.get("/health")
async def health_check():
    return {"status": "ok"}

# Include the routers
app.include_router(faces.router, prefix="/api/v1", tags=["faces"])
app.include_router(knowledge.router, prefix="/api/v1", tags=["knowledge"])
app.include_router(search.router, prefix="/api/v1", tags=["search"])

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)