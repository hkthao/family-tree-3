from typing import List, Dict, Any, Optional
from PIL import Image
import logging

from src.domain.interfaces.face_repository import IFaceRepository
from src.domain.interfaces.face_embedding import IFaceEmbedding

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)


class FaceManager:
    def __init__(self, face_repository: IFaceRepository, face_embedding_service: IFaceEmbedding):
        self.face_repository = face_repository
        self.face_embedding_service = face_embedding_service

    async def add_face(self, face_image: Image.Image, metadata: Dict[str, Any]) -> Dict[str, Any]:
        """
        Thêm một khuôn mặt mới vào hệ thống, tạo embedding và lưu trữ vào Qdrant.
        Metadata phải chứa 'memberId' và 'familyId'.
        """
        if "member_id" not in metadata or "family_id" not in metadata:
            raise ValueError("Metadata phải chứa 'member_id' và 'family_id'.")

        embedding = self.face_embedding_service.get_embedding(face_image)

        if "face_id" not in metadata:
            raise ValueError("Metadata phải chứa 'face_id'.")
        face_id = metadata["face_id"]

        await self.face_repository.upsert_face_vector(face_id, embedding, metadata)
        logger.info(f"Đã thêm khuôn mặt {face_id} cho member {metadata['member_id']} trong family {metadata['family_id']}.")
        return {"face_id": face_id, "embedding": embedding, "metadata": metadata}

    async def get_faces_by_family_id(self, family_id: str) -> List[Dict[str, Any]]:
        """
        Lấy tất cả các khuôn mặt thuộc về một family_id cụ thể.
        """
        faces_data = await self.face_repository.get_faces_by_family_id(family_id)
        logger.info(f"Đã truy xuất {len(faces_data)} khuôn mặt cho family {family_id}.")
        return faces_data

    async def delete_face(self, face_id: str) -> bool:
        """
        Xóa một khuôn mặt dựa trên face_id.
        """
        success = await self.face_repository.delete_face(face_id)
        if success:
            logger.info(f"Đã xóa khuôn mặt với face_id: {face_id}.")
        else:
            logger.warning(f"Không tìm thấy hoặc không thể xóa khuôn mặt với faceId: {face_id}.")
        return success

    async def delete_faces_by_family_id(self, family_id: str) -> bool:
        """
        Xóa tất cả các khuôn mặt thuộc về một family_id cụ thể.
        """
        logger.info(f"Đang xóa các khuôn mặt cho family {family_id}...")
        success = await self.face_repository.delete_faces_by_family_id(family_id)
        if success:
            logger.info(f"Đã xóa thành công các khuôn mặt cho family {family_id}.")
        else:
            logger.warning(f"Không thể xóa các khuôn mặt cho family {family_id}.")
        return success

    async def add_face_by_vector(self, vector: List[float], metadata: Dict[str, Any]) -> Dict[str, Any]:
        """
        Thêm một khuôn mặt mới vào hệ thống trực tiếp bằng vector embedding và metadata.
        Metadata phải chứa 'memberId' và 'familyId'.
        """
        if "member_id" not in metadata or "family_id" not in metadata:
            raise ValueError("Metadata phải chứa 'member_id' và 'family_id'.")

        if "face_id" not in metadata:
            raise ValueError("Metadata phải chứa 'face_id'.")
        face_id = metadata["face_id"]

        await self.face_repository.upsert_face_vector(face_id, vector, metadata)
        logger.info(f"Đã thêm khuôn mặt {face_id} (từ vector) cho member {metadata['member_id']} trong family {metadata['family_id']}.")
        return {"face_id": face_id, "embedding": vector, "metadata": metadata}

    async def search_similar_faces(self, face_image: Image.Image, family_id: Optional[str] = None, limit: int = 5, threshold: float = 0.7) -> List[Dict[str, Any]]:
        """
        Tìm kiếm các khuôn mặt tương tự trong Qdrant.
        Có thể lọc theo family_id.
        """
        query_embedding = self.face_embedding_service.get_embedding(face_image)

        search_results = await self.face_repository.search_similar_faces(
            query_embedding,
            family_id=family_id,
            top_k=limit,
            threshold=threshold
        )
        logger.info(f"Đã tìm thấy {len(search_results)} khuôn mặt tương tự cho query.")
        return search_results

    async def search_similar_faces_by_vector(self, query_embedding: List[float], family_id: Optional[str] = None, member_id: Optional[str] = None, limit: int = 5, threshold: float = 0.7) -> List[Dict[str, Any]]:
        """
        Tìm kiếm các khuôn mặt tương tự trong Qdrant sử dụng vector embedding trực tiếp.
        Có thể lọc theo family_id và member_id.
        """
        search_results = await self.face_repository.search_similar_faces(
            query_embedding,
            family_id=family_id,
            member_id=member_id,
            top_k=limit,
            threshold=threshold
        )
        logger.info(f"Đã tìm thấy {len(search_results)} khuôn mặt tương tự từ vector query.")
        return search_results
