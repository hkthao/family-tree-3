import os
from qdrant_client import QdrantClient, models
from qdrant_client.http.models import UpdateStatus
from typing import List, Dict, Any
from loguru import logger

from ..config import TEXT_EMBEDDING_DIMENSIONS
from ..core.embeddings import EmbeddingService
from ..schemas.vectors import (
    VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
)


class KnowledgeQdrantService:
    def __init__(self, embedding_service: EmbeddingService):
        self.embedding_service = embedding_service
        self.client = QdrantClient(
            host=os.getenv("QDRANT_HOST"),
            api_key=os.getenv("QDRANT_API_KEY"),
        )
        self.collection_name = os.getenv("QDRANT_KNOWLEDGE_COLLECTION_NAME", "knowledge_embeddings")
        self._create_collection_if_not_exists()

    def _create_collection_if_not_exists(self):
        try:
            self.client.get_collection(collection_name=self.collection_name)
            logger.info(f"Collection '{self.collection_name}' already exists.")
        except Exception:
            logger.info(f"Collection '{self.collection_name}' not found. Creating it...")
            self.client.recreate_collection(
                collection_name=self.collection_name,
                vectors_config=models.VectorParams(
                    size=TEXT_EMBEDDING_DIMENSIONS,
                    distance=models.Distance.COSINE
                ),
            )
            logger.info(f"Collection '{self.collection_name}' created successfully.")

    async def add_vectors(self, vectors_data: List[VectorData]):
        if not vectors_data:
            logger.warning("No vector data provided to add.")
            return

        points = []
        for v_data in vectors_data:
            item = v_data.model_dump(exclude_none=True)
            vector = self.embedding_service.embed_query(item["summary"])

            # Use a unique ID for the point. Assuming 'entity_id' is unique within 'family_id'
            # For Qdrant, a point ID can be a string or integer. Let's use a combination for uniqueness.
            # Qdrant recommends using UUID for point IDs.
            point_id = f"{item['family_id']}-{item['entity_id']}"

            # Prepare payload. Qdrant payload is a dict.
            # LanceDB had 'metadata' as JSON string, Qdrant can store dict directly.
            payload = {
                "family_id": item["family_id"],
                "entity_id": item["entity_id"],
                "type": item["type"],
                "visibility": item["visibility"],
                "name": item["name"],
                "summary": item["summary"],
                **item.get("metadata", {})  # Merge additional metadata
            }

            points.append(
                models.PointStruct(
                    id=point_id,
                    vector=vector,
                    payload=payload,
                )
            )

        self.client.upsert(
            collection_name=self.collection_name,
            wait=True,
            points=points
        )
        logger.info(f"Added {len(vectors_data)} vectors to collection '{self.collection_name}'.")

    async def update_vectors(self, update_request: UpdateVectorRequest):
        point_id = f"{update_request.family_id}-{update_request.entity_id}"

        # Fetch current payload to merge updates
        try:
            current_point = self.client.retrieve(
                collection_name=self.collection_name,
                ids=[point_id],
                with_payload=True,
                with_vectors=False
            ).pop()
            current_payload = current_point.payload
        except IndexError:
            logger.warning(f"Point with ID '{point_id}' not found for update.")
            return

        updates = update_request.model_dump(exclude_unset=True,
                                            exclude={'family_id', 'entity_id'})

        if not updates:
            logger.warning("No update fields provided. Nothing to update.")
            return

        new_payload = current_payload.copy()

        # Merge new metadata with existing metadata if present
        if 'metadata' in updates:
            new_payload.update(updates.pop('metadata'))

        # Merge remaining updates (like 'summary')
        new_payload.update(updates)

        # If summary is updated, generate a new vector
        if 'summary' in updates:
            new_vector = self.embedding_service.embed_query(new_payload['summary'])
        else:
            new_vector = None  # Keep original vector

        self.client.upsert(
            collection_name=self.collection_name,
            wait=True,
            points=[
                models.PointStruct(
                    id=point_id,
                    vector=new_vector if new_vector else current_point.vector,
                    payload=new_payload,
                )
            ]
        )
        logger.info(f"Updated point '{point_id}' in collection '{self.collection_name}'.")

    async def delete_vectors(self, delete_request: DeleteVectorRequest) -> int:
        qdrant_filter_conditions = [
            models.FieldCondition(
                key="family_id",
                match=models.MatchValue(value=delete_request.family_id)
            )
        ]

        if delete_request.entity_id is not None:
            qdrant_filter_conditions.append(
                models.FieldCondition(
                    key="entity_id",
                    match=models.MatchValue(value=delete_request.entity_id)
                )
            )
        elif delete_request.type is not None:
            qdrant_filter_conditions.append(
                models.FieldCondition(
                    key="type",
                    match=models.MatchValue(value=delete_request.type)
                )
            )
        elif delete_request.where_clause:
            # Qdrant filters are structured, converting a raw where_clause
            # string directly is not straightforward.
            # This would require parsing the SQL-like string into Qdrant filter
            # objects, which is complex. For now, we'll only support
            # simple key-value filters.
            logger.warning("Raw where_clause not supported for deletion in Qdrant. "
                           "Only family_id, entity_id, or type filters are applied.")
            # We could raise an error here if strict adherence to where_clause is required.
            # For this refactor, we'll proceed with supported filters only.

        response = self.client.delete(
            collection_name=self.collection_name,
            points_selector=models.FilterSelector(
                filter=models.Filter(
                    must=qdrant_filter_conditions
                )
            ),
            wait=True
        )
        if response.status == UpdateStatus.COMPLETED:
            logger.info(f"Deleted points with filter {qdrant_filter_conditions} successfully.")
            return response.count  # Qdrant delete response usually includes count
        else:
            logger.warning(f"Failed to delete points with filter {qdrant_filter_conditions}. Status: {response.status}")
            return 0

    async def delete_knowledge_by_family_id(self, family_id: str) -> None:
        qdrant_filter_conditions = [
            models.FieldCondition(
                key="family_id",
                match=models.MatchValue(value=family_id)
            )
        ]
        response = self.client.delete(
            collection_name=self.collection_name,
            points_selector=models.FilterSelector(
                filter=models.Filter(
                    must=qdrant_filter_conditions
                )
            ),
            wait=True
        )
        if response.status == UpdateStatus.COMPLETED:
            logger.info(f"Deleted all knowledge for family_id '{family_id}' successfully.")
        else:
            logger.warning(f"Failed to delete knowledge for family_id '{family_id}'. Status: {response.status}")

    async def rebuild_vectors(self, rebuild_request: RebuildVectorRequest):
        qdrant_filter_conditions = [
            models.FieldCondition(
                key="family_id",
                match=models.MatchValue(value=rebuild_request.family_id)
            )
        ]
        if rebuild_request.entity_ids:
            qdrant_filter_conditions.append(
                models.FieldCondition(
                    key="entity_id",
                    match=models.MatchAny(any=rebuild_request.entity_ids)
                )
            )

        # Retrieve points to re-embed
        # This will fetch max 10000 points. If more, pagination is needed.
        points_to_rebuild, _ = self.client.scroll(
            collection_name=self.collection_name,
            scroll_filter=models.Filter(must=qdrant_filter_conditions),
            limit=10000,
            with_payload=True,
            with_vectors=True  # Need current vectors to reconstruct PointStruct
        )

        if not points_to_rebuild:
            logger.warning(f"No entries found for rebuilding with filter {qdrant_filter_conditions}.")
            return

        re_embedded_points = []
        for point in points_to_rebuild:
            summary = point.payload.get("summary")
            if summary:
                new_vector = self.embedding_service.embed_query(summary)
                re_embedded_points.append(
                    models.PointStruct(
                        id=point.id,
                        vector=new_vector,
                        payload=point.payload  # Keep existing payload
                    )
                )
            else:
                logger.warning(f"Point {point.id} has no summary to re-embed. Skipping.")

        if re_embedded_points:
            self.client.upsert(
                collection_name=self.collection_name,
                wait=True,
                points=re_embedded_points
            )
            logger.info(f"Rebuilt and upserted {len(re_embedded_points)} vectors in collection '{self.collection_name}'.")
        else:
            logger.info("No vectors were re-embedded.")

    async def search_knowledge_table(
        self,
        family_id: str,
        query_vector: List[float],
        allowed_visibility: List[str],
        top_k: int
    ) -> List[Dict[str, Any]]:
        qdrant_filter_conditions = [
            models.FieldCondition(
                key="family_id",
                match=models.MatchValue(value=family_id)
            ),
            models.FieldCondition(
                key="visibility",
                match=models.MatchAny(any=allowed_visibility)
            )
        ]

        search_result = self.client.search(
            collection_name=self.collection_name,
            query_vector=query_vector,
            query_filter=models.Filter(must=qdrant_filter_conditions),
            limit=top_k,
            with_payload=True
        )

        formatted_results = []
        for hit in search_result:
            formatted_results.append({
                "metadata": hit.payload,  # Qdrant payload is already the metadata
                "summary": hit.payload.get("summary"),
                "score": hit.score
            })
        return formatted_results
