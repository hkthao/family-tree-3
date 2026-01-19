from fastapi import FastAPI
from app.api import faces, knowledge, search # Import the routers

app = FastAPI(title="Knowledge Search Service API") # Updated title

@app.get("/health")
async def health_check():
    return {"status": "ok"}

# Include the routers
app.include_router(faces.router, prefix="/api/v1", tags=["faces"])
app.include_router(knowledge.router, prefix="/api/v1/knowledge", tags=["knowledge"])
app.include_router(search.router, prefix="/api/v1", tags=["search"])

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)