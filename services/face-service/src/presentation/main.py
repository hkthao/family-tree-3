from fastapi import FastAPI
import uvicorn
import logging
import asyncio
from contextlib import asynccontextmanager

from src.infrastructure.message_bus.consumer_impl import MessageConsumer
from src.presentation.dependencies import get_message_consumer
from src.presentation.api.v1.endpoints import face_endpoints

# Configure logging
logging.basicConfig(
    level=logging.INFO, format=("%(asctime)s - %(levelname)s - %(message)s")
)
logger = logging.getLogger(__name__)

@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("Starting up application and message consumer...")
    message_consumer: MessageConsumer = get_message_consumer()
    asyncio.create_task(message_consumer.start())
    yield
    logger.info("Shutting down application and message consumer...")
    await message_consumer.stop()

app = FastAPI(
    title="ImageFaceService",
    description=(
        "A FastAPI service for face detection, embedding, and cropping. "
        "Also provides face management and search using Qdrant."
    ),
    version="1.0.0",
    lifespan=lifespan,
)

app.include_router(face_endpoints.router, prefix="")

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
