from typing import List, Dict, Any, Optional
from PIL import Image
import numpy as np
import logging

from src.domain.interfaces.face_repository import IFaceRepository
from src.domain.interfaces.face_embedding import IFaceEmbedding
from src.domain.interfaces.face_detector import IFaceDetector

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)


class FaceManager:
    def __init__(self, face_repository: IFaceRepository, face_embedding_service: IFaceEmbedding, face_detector_service: IFaceDetector):
        self.face_repository = face_repository
        self.face_embedding_service = face_embedding_service
        self.face_detector_service = face_detector_service

    def detect_faces_in_image(self, image_np: np.ndarray) -> List[Dict[str, Any]]:
        """
        Phát hiện khuôn mặt trong một hình ảnh NumPy array.
        """
        return self.face_detector_service.detect_faces(image_np)

    def detect_and_embed_faces(self, image: Image.Image) -> List[Dict[str, Any]]:
        """
        Phát hiện các khuôn mặt trong một ảnh, cắt từng khuôn mặt và tạo embedding cho chúng.
        Args:
            image (Image.Image): Ảnh đầu vào dưới dạng đối tượng PIL Image.
        Returns:
            List[Dict[str, Any]]: Danh sách các từ điển, mỗi từ điển chứa
                                  'box' (bounding box), 'confidence' (điểm tin cậy),
                                  và 'embedding' (vector nhúng).
        """
        # Convert PIL Image to numpy array for detection
        image_np = np.array(image)

        detected_faces_data = self.face_detector_service.detect_faces(image_np)
        
        results = []
        for face_data in detected_faces_data:
            x, y, w, h = [int(val) for val in face_data['box']]
            
            # Đảm bảo tọa độ hợp lệ và không vượt ra ngoài biên ảnh
            # Thêm một khoảng đệm nhỏ (ví dụ: 10% chiều rộng/chiều cao)
            padding_w = int(w * 0.1)
            padding_h = int(h * 0.1)

            x1 = max(0, x - padding_w)
            y1 = max(0, y - padding_h)
            x2 = min(image.width, x + w + padding_w)
            y2 = min(image.height, y + h + padding_h)

            # Cắt khuôn mặt từ ảnh PIL
            cropped_face_image = image.crop((x1, y1, x2, y2))
            
            # Debug: Log the size of the cropped face image
            logger.debug(f"Kích thước ảnh khuôn mặt đã cắt (có đệm): {cropped_face_image.size}")
            
            # Tạo embedding cho khuôn mặt đã cắt
            embedding = self.face_embedding_service.get_embedding(cropped_face_image)

            if not embedding:
                logger.warning(f"Embedding trả về rỗng cho khuôn mặt tại hộp: {[x1, y1, x2 - x1, y2 - y1]}. Bỏ qua khuôn mặt này.")
                continue # Bỏ qua khuôn mặt nếu embedding trống
            
            results.append({
                'box': [x1, y1, x2 - x1, y2 - y1], # Return box in x, y, w, h format
                'confidence': face_data['confidence'],
                'embedding': embedding
            })
        return results

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

    async def search_similar_faces_by_vectors(self, query_embeddings: List[List[float]], family_id: Optional[str] = None, limit: int = 5, threshold: float = 0.7) -> List[List[Dict[str, Any]]]:
        """
        Tìm kiếm các khuôn mặt tương tự trong Qdrant sử dụng một danh sách các vector embedding trực tiếp.
        Có thể lọc theo family_id.
        """
        batch_search_results = await self.face_repository.batch_search_similar_faces(
            query_embeddings,
            family_id=family_id,
            top_k=limit,
            threshold=threshold
        )
        logger.info(f"Đã tìm thấy {len(batch_search_results)} kết quả tìm kiếm hàng loạt từ các vector query.")
        return batch_search_results
