from typing import List, Dict, Any, Optional
from app.services.qdrant_service import QdrantService
from app.services.face_embedding import FaceEmbeddingService
from PIL import Image
import logging

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)


class FaceService:
    def __init__(self, qdrant_service: QdrantService, face_embedding_service: FaceEmbeddingService):
        self.qdrant_service = qdrant_service
        self.face_embedding_service = face_embedding_service

    def add_face(self, face_image: Image.Image, metadata: Dict[str, Any]) -> Dict[str, Any]:
        """
        Thêm một khuôn mặt mới vào hệ thống, tạo embedding và lưu trữ vào Qdrant.
        Metadata phải chứa 'memberId' và 'familyId'.
        """
        if "memberId" not in metadata or "familyId" not in metadata:
            raise ValueError("Metadata phải chứa 'memberId' và 'familyId'.")

        embedding = self.face_embedding_service.get_embedding(face_image)

        if "faceId" not in metadata:
            raise ValueError("Metadata phải chứa 'faceId'.")
        face_id = metadata["faceId"]

        self.qdrant_service.upsert_face_embedding(embedding, metadata, face_id)
        logger.info(f"Đã thêm khuôn mặt {face_id} cho member {metadata['memberId']} trong family {metadata['familyId']}.")
        return {"faceId": face_id, "embedding": embedding, "metadata": metadata}

    def get_faces_by_family_id(self, family_id: str) -> List[Dict[str, Any]]:
        """
        Lấy tất cả các khuôn mặt thuộc về một family_id cụ thể.
        Sử dụng bộ lọc payload của Qdrant.
        """
        # Qdrant search with filter
        # Note: This will return points, not just metadata.
        # We might need to adjust qdrant_service.search_face_embeddings or create a new method there.
        # For now, let's assume qdrant_service can filter by payload.
        # This will be refined as we implement the search in QdrantService.

        # Placeholder for Qdrant payload filter
        # In Qdrant, you would typically use `client.scroll` with a filter for this
        # or `client.search` with a filter if you have a query vector (which we don't for 'get all')

        # For simplicity, let's assume a scroll-like operation to get all points matching the filter.
        # A more robust solution might involve creating a dedicated method in QdrantService for this.

        # Dummy call for now, will refine based on QdrantClient capabilities
        # This part requires more thought on how QdrantClient allows fetching by filter without a vector.
        # For now, I will create a dummy QdrantClient.scroll in the qdrant_service
        # (This will be implemented in the QdrantService next)

        # Assuming a method like this in QdrantService:
        faces_data = self.qdrant_service.get_points_by_payload_filter(
            payload_filter={"familyId": family_id}
        )
        logger.info(f"Đã truy xuất {len(faces_data)} khuôn mặt cho family {family_id}.")
        return faces_data

    def delete_face(self, face_id: str) -> bool:
        """
        Xóa một khuôn mặt dựa trên face_id.
        """
        success = self.qdrant_service.delete_point_by_id(face_id)
        if success:
            logger.info(f"Đã xóa khuôn mặt với faceId: {face_id}.")
        else:
            logger.warning(f"Không tìm thấy hoặc không thể xóa khuôn mặt với faceId: {face_id}.")
        return success

    def delete_faces_by_family_id(self, family_id: str) -> bool:
        """
        Xóa tất cả các khuôn mặt thuộc về một family_id cụ thể.
        """
        logger.info(f"Đang xóa các khuôn mặt cho family {family_id}...")
        success = self.qdrant_service.delete_points_by_payload_filter(
            payload_filter={"familyId": family_id}
        )
        if success:
            logger.info(f"Đã xóa thành công các khuôn mặt cho family {family_id}.")
        else:
            logger.warning(f"Không thể xóa các khuôn mặt cho family {family_id}.")
        return success

    def add_face_by_vector(self, vector: List[float], metadata: Dict[str, Any]) -> Dict[str, Any]:
        """
        Thêm một khuôn mặt mới vào hệ thống trực tiếp bằng vector embedding và metadata.
        Metadata phải chứa 'memberId' và 'familyId'.
        """
        if "memberId" not in metadata or "familyId" not in metadata:
            raise ValueError("Metadata phải chứa 'memberId' và 'familyId'.")

        if "faceId" not in metadata:
            raise ValueError("Metadata phải chứa 'faceId'.")
        face_id = metadata["faceId"]

        self.qdrant_service.upsert_face_embedding(vector, metadata, face_id)
        logger.info(f"Đã thêm khuôn mặt {face_id} (từ vector) cho member {metadata['memberId']} trong family {metadata['familyId']}.")
        return {"faceId": face_id, "embedding": vector, "metadata": metadata}

    def search_similar_faces(self, face_image: Image.Image, family_id: Optional[str] = None, limit: int = 5) -> List[Dict[str, Any]]:
        """
        Tìm kiếm các khuôn mặt tương tự trong Qdrant.
        Có thể lọc theo family_id.
        """
        query_embedding = self.face_embedding_service.get_embedding(face_image)

        search_filter = None
        if family_id:
            search_filter = {"familyId": family_id}

        search_results = self.qdrant_service.search_face_embeddings(
            query_vector=query_embedding,
            limit=limit,
            query_filter=search_filter
        )
        logger.info(f"Đã tìm thấy {len(search_results)} khuôn mặt tương tự cho query.")
        return search_results

    def search_similar_faces_by_vector(self, query_embedding: List[float], family_id: Optional[str] = None, member_id: Optional[str] = None, limit: int = 5, threshold: float = 0.7) -> List[Dict[str, Any]]:
        """
        Tìm kiếm các khuôn mặt tương tự trong Qdrant sử dụng vector embedding trực tiếp.
        Có thể lọc theo family_id và member_id.
        """
        search_filter = {}
        if family_id:
            search_filter["familyId"] = family_id
        if member_id:
            search_filter["memberId"] = member_id

        # Convert empty search_filter to None if no filters are applied
        final_filter = search_filter if search_filter else None

        search_results = self.qdrant_service.search_face_embeddings(
            query_vector=query_embedding,
            limit=limit,
            query_filter=final_filter,
            score_threshold=threshold  # Pass threshold to Qdrant service
        )
        logger.info(f"Đã tìm thấy {len(search_results)} khuôn mặt tương tự từ vector query.")
        return search_results
