import os
from fastapi import Depends
from dotenv import load_dotenv

from src.domain.interfaces.face_detector import IFaceDetector
from src.domain.interfaces.face_embedding import IFaceEmbedding
from src.domain.interfaces.face_repository import IFaceRepository

from src.infrastructure.detectors.dlib_detector import DlibFaceDetector
from src.infrastructure.detectors.retinaface_detector import RetinaFaceDetector
from src.infrastructure.embeddings.facenet_embedding import FaceNetEmbeddingService
from src.infrastructure.embeddings.arcface_embedding import ArcFaceEmbedding # Import ArcFaceEmbedding
from src.infrastructure.persistence.qdrant_client import QdrantFaceRepository
from src.infrastructure.message_bus.consumer_impl import MessageConsumer

from src.application.services.face_manager import FaceManager

# Tải các biến môi trường từ tệp .env
load_dotenv()

def get_face_detector() -> IFaceDetector:
    detector_model = os.getenv("FACE_DETECTOR_MODEL", "dlib").lower()
    if detector_model == "retinaface":
        return RetinaFaceDetector()
    elif detector_model == "dlib":
        return DlibFaceDetector()
    else:
        raise ValueError(f"Mô hình phát hiện khuôn mặt không hợp lệ: {detector_model}. Chỉ chấp nhận 'dlib' hoặc 'retinaface'.")

def get_face_embedding_service() -> IFaceEmbedding:
    embedding_model = os.getenv("FACE_EMBEDDING_MODEL", "facenet").lower() # Mặc định là facenet nếu không có trong .env
    if embedding_model == "arcface":
        return ArcFaceEmbedding()
    elif embedding_model == "facenet":
        return FaceNetEmbeddingService()
    else:
        raise ValueError(f"Mô hình nhúng khuôn mặt không hợp lệ: {embedding_model}. Chỉ chấp nhận 'facenet' hoặc 'arcface'.")

def get_face_repository() -> IFaceRepository:
    return QdrantFaceRepository()

def get_face_manager(
    face_repository: IFaceRepository = Depends(get_face_repository),
    face_embedding_service: IFaceEmbedding = Depends(get_face_embedding_service),
    face_detector_service: IFaceDetector = Depends(get_face_detector),
) -> FaceManager:
    return FaceManager(face_repository, face_embedding_service, face_detector_service)

def get_message_consumer(
    face_manager: FaceManager = Depends(get_face_manager)
) -> MessageConsumer:
    return MessageConsumer(face_manager)

