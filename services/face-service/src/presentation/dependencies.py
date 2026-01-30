from fastapi import Depends

from src.domain.interfaces.face_detector import IFaceDetector
from src.domain.interfaces.face_embedding import IFaceEmbedding
from src.domain.interfaces.face_repository import IFaceRepository

from src.infrastructure.detectors.dlib_detector import DlibFaceDetector
from src.infrastructure.embeddings.facenet_embedding import FaceNetEmbeddingService
from src.infrastructure.persistence.qdrant_client import QdrantFaceRepository
from src.infrastructure.message_bus.consumer_impl import MessageConsumer

from src.application.services.face_manager import FaceManager

def get_face_detector() -> IFaceDetector:
    return DlibFaceDetector()

def get_face_embedding_service() -> IFaceEmbedding:
    return FaceNetEmbeddingService()

def get_face_repository() -> IFaceRepository:
    return QdrantFaceRepository()

def get_face_manager(
    face_repository: IFaceRepository = Depends(get_face_repository),
    face_embedding_service: IFaceEmbedding = Depends(get_face_embedding_service),
) -> FaceManager:
    return FaceManager(face_repository, face_embedding_service)

def get_message_consumer(
    face_manager: FaceManager = Depends(get_face_manager)
) -> MessageConsumer:
    return MessageConsumer(face_manager)
