import os
from qdrant_client import QdrantClient, models
from qdrant_client.http.models import UpdateStatus  # Import UpdateStatus
from typing import List, Dict, Any, Optional
import logging

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)


class QdrantService:
    def __init__(self, collection_name: Optional[str] = None):
        self.collection_name = collection_name or os.getenv("QDRANT_COLLECTION_NAME", "face_embeddings")
        self.client = QdrantClient(
            host=os.getenv("QDRANT_HOST"),
            api_key=os.getenv("QDRANT_API_KEY"),
        )
        self._create_collection_if_not_exists()


    def _create_collection_if_not_exists(self):
        try:
            # Always try to delete the collection first to ensure a fresh start with the new index
            self.client.delete_collection(collection_name=self.collection_name)
            logger.info(f"Collection '{self.collection_name}' deleted (if it existed) to apply new index.")
        except Exception as e:
            logger.info(f"Collection '{self.collection_name}' did not exist or could not be deleted: {e}")

        logger.info(f"Creating collection '{self.collection_name}' with familyId index...")
        self.client.recreate_collection(
            collection_name=self.collection_name,
            vectors_config=models.VectorParams(size=128, distance=models.Distance.COSINE),
        )
        self.client.create_payload_index(
            collection_name=self.collection_name,
            field_name="familyId",
                            field_schema=models.PayloadSchemaType.KEYWORD,        )
        logger.info(f"Collection '{self.collection_name}' created successfully with familyId index.")

    def upsert_face_embedding(self, vector: List[float], metadata: Dict[str, Any], point_id: str):
        """
        Lưu trữ embedding khuôn mặt và metadata vào Qdrant.
        point_id sẽ được dùng làm ID của point trong Qdrant.
        """
        points = [
            models.PointStruct(
                id=point_id,
                vector=vector,
                payload=metadata,
            )
        ]
        self.client.upsert(
            collection_name=self.collection_name,
            wait=True,
            points=points
        )
        logger.info(f"Upserted embedding for point_id: {point_id} to collection '{self.collection_name}'.")

    def search_face_embeddings(self, query_vector: List[float], limit: int = 5, query_filter: Optional[Dict[str, Any]] = None, score_threshold: float = 0.0) -> List[Dict[str, Any]]:
        """
        Tìm kiếm các khuôn mặt tương tự dựa trên vector truy vấn.
        Có thể lọc kết quả tìm kiếm bằng query_filter.
        """
        qdrant_filter = None
        if query_filter:
            qdrant_filter = models.Filter(
                must=[
                    models.FieldCondition(
                        key=k,
                        match=models.MatchValue(value=v)
                    ) for k, v in query_filter.items()
                ]
            )

        search_result_raw = self.client.query_points(
            collection_name=self.collection_name,
            query=query_vector,
            limit=limit,
            query_filter=qdrant_filter,
            score_threshold=score_threshold
        )
        
        search_hits = search_result_raw.points
        
        results = []
        for hit in search_hits:
            results.append({
                "id": hit.id,
                "score": hit.score,
                "payload": hit.payload
            })
        return results

    def get_points_by_payload_filter(self, payload_filter: Dict[str, Any]) -> List[Dict[str, Any]]:
        """
        Lấy tất cả các điểm khớp với một payload filter cụ thể.
        """
        qdrant_filter = models.Filter(
            must=[
                models.FieldCondition(
                    key=k,
                    match=models.MatchValue(value=v)
                ) for k, v in payload_filter.items()
            ]
        )

        # Using scroll to get all points matching the filter
        hits, _ = self.client.scroll(
            collection_name=self.collection_name,
            scroll_filter=qdrant_filter,
            limit=10000,  # Adjust limit as necessary, or implement pagination
            with_payload=True,
            with_vectors=False,  # We don't need vectors for this operation
        )
        results = []
        for hit in hits:
            results.append({
                "id": hit.id,
                "payload": hit.payload
            })
        logger.info(f"Retrieved {len(results)} points with filter {payload_filter}.")
        return results

    def delete_points_by_payload_filter(self, payload_filter: Dict[str, Any]) -> bool:
        """
        Xóa tất cả các điểm khớp với một payload filter cụ thể.
        """
        qdrant_filter = models.Filter(
            must=[
                models.FieldCondition(
                    key=k,
                    match=models.MatchValue(value=v)
                ) for k, v in payload_filter.items()
            ]
        )
        try:
            response = self.client.delete(
                collection_name=self.collection_name,
                points_selector=models.PointSelector(
                    filter=qdrant_filter
                ),
                wait=True
            )
            if response.status == UpdateStatus.COMPLETED:
                logger.info(f"Deleted points with filter {payload_filter} successfully.")
                return True
            else:
                logger.warning(f"Failed to delete points with filter {payload_filter}. Status: {response.status}")
                return False
        except Exception as e:
            logger.error(f"Error deleting points with filter {payload_filter}: {e}")
            return False

    def delete_point_by_id(self, point_id: str) -> bool:
        """
        Xóa một điểm khỏi Qdrant bằng ID của nó.
        """
        try:
            response = self.client.delete(
                collection_name=self.collection_name,
                points=[point_id],  # Correct way to specify point IDs
                wait=True
            )
            if response.status == UpdateStatus.COMPLETED:
                logger.info(f"Point with ID {point_id} deleted successfully.")
                return True
            else:
                logger.warning(f"Failed to delete point with ID {point_id}. Status: {response.status}")
                return False
        except Exception as e:
            logger.error(f"Error deleting point with ID {point_id}: {e}")
            return False
