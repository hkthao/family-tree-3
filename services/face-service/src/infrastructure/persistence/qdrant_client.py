import os
from qdrant_client import QdrantClient, models
from qdrant_client.http.models import UpdateStatus
from typing import List, Dict, Any, Optional
import logging

from src.domain.interfaces.face_repository import IFaceRepository

logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)


class QdrantFaceRepository(IFaceRepository):
    def __init__(self, collection_name: Optional[str] = None):
        self.collection_name = collection_name or os.getenv("QDRANT_COLLECTION_NAME", "face_embeddings")
        self.vector_size = int(os.getenv("QDRANT_VECTOR_SIZE", 128))
        self.client = QdrantClient(
            host=os.getenv("QDRANT_HOST"),
            api_key=os.getenv("QDRANT_API_KEY"),
        )
        self._create_collection_if_not_exists()

    def _create_collection_if_not_exists(self):
        if not self.client.collection_exists(collection_name=self.collection_name):
            logger.info(f"Collection '{self.collection_name}' does not exist. Creating it now...")
            self.client.create_collection(
                collection_name=self.collection_name,
                vectors_config=models.VectorParams(size=self.vector_size, distance=models.Distance.COSINE),
            )
            self.client.create_payload_index(
                collection_name=self.collection_name,
                field_name="family_id",
                field_schema=models.PayloadSchemaType.KEYWORD,
            )
            logger.info(f"Collection '{self.collection_name}' created successfully with family_id index.")
        else:
            logger.info(f"Collection '{self.collection_name}' already exists. Skipping creation.")

    async def upsert_face_vector(self, face_id: str, vector: List[float], metadata: Dict[str, Any]):
        """
        Inserts or updates a face vector and its associated metadata in the repository.
        """
        points = [
            models.PointStruct(
                id=face_id,
                vector=vector,
                payload=metadata,
            )
        ]
        self.client.upsert(
            collection_name=self.collection_name,
            wait=True,
            points=points
        )
        logger.info(f"Upserted embedding for point_id: {face_id} to collection '{self.collection_name}'.")

    async def search_similar_faces(
        self,
        query_vector: List[float],
        family_id: Optional[str] = None,
        member_id: Optional[str] = None,
        top_k: int = 5,
        threshold: float = 0.75,
    ) -> List[Dict[str, Any]]:
        """
        Searches for similar faces based on a query vector.
        """
        qdrant_filter_conditions = []

        if family_id:
            qdrant_filter_conditions.append(
                models.FieldCondition(
                    key="family_id",
                    match=models.MatchValue(value=family_id)
                )
            )
        if member_id:
            qdrant_filter_conditions.append(
                models.FieldCondition(
                    key="member_id",
                    match=models.MatchValue(value=member_id)
                )
            )

        qdrant_filter = None
        if qdrant_filter_conditions:
            qdrant_filter = models.Filter(
                must=qdrant_filter_conditions
            )

        search_result_raw = self.client.query_points(
            collection_name=self.collection_name,
            query=query_vector,
            limit=top_k,
            query_filter=qdrant_filter,
            score_threshold=threshold, # Use score_threshold here
        )
        
        search_hits = search_result_raw.points
        
        results = []
        for hit in search_hits:
            results.append({
                    "id": hit.id,
                    "score": hit.score,
                    "payload": hit.payload
                })   
        logger.info(f"Qdrant search completed. Found {len(results)} hits above threshold. Results: "
                    f"{[{'id': r['id'], 'score': r['score']} for r in results]}")
        return results

    async def get_faces_by_family_id(self, family_id: str) -> List[Dict[str, Any]]:
        """
        Retrieves all faces associated with a given family ID.
        """
        payload_filter = {"family_id": family_id}
        qdrant_filter = models.Filter(
            must=[
                models.FieldCondition(
                    key=k,
                    match=models.MatchValue(value=v)
                ) for k, v in payload_filter.items()
            ]
        )

        hits, _ = self.client.scroll(
            collection_name=self.collection_name,
            scroll_filter=qdrant_filter,
            limit=10000,
            with_payload=True,
            with_vectors=False,
        )
        results = []
        for hit in hits:
            results.append({
                "id": hit.id,
                "payload": hit.payload
            })
        logger.info(f"Retrieved {len(results)} points with filter {payload_filter}.")
        return results

    async def delete_face(self, face_id: str) -> bool:
        """
        Deletes a specific face by its ID.
        """
        try:
            response = self.client.delete(
                collection_name=self.collection_name,
                points_selector=models.PointIdsList(points=[face_id]),
                wait=True
            )
            if response.status == UpdateStatus.COMPLETED:
                logger.info(f"Point with ID {face_id} deleted successfully.")
                return True
            else:
                logger.warning(f"Failed to delete point with ID {face_id}. Status: {response.status}")
                return False
        except Exception as e:
            logger.error(f"Error deleting point with ID {face_id}: {e}")
            return False

    async def delete_faces_by_family_id(self, family_id: str) -> bool:
        """
        Deletes all faces associated with a given family ID.
        """
        payload_filter = {"family_id": family_id}
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
    
    async def delete_faces_by_member_id(self, member_id: str) -> bool:
        """
        Deletes all faces associated with a given member ID.
        """
        payload_filter = {"member_id": member_id}
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
