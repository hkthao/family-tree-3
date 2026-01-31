import os
from qdrant_client import AsyncQdrantClient, models as qdrant_models
from qdrant_client.http.models import UpdateStatus
from typing import List, Dict, Any
import uuid
from loguru import logger
from ..config import TEXT_EMBEDDING_DIMENSIONS
from ..core.embeddings import EmbeddingService
from ..schemas.vectors import (
    VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
)


class KnowledgeQdrantService:
    def __init__(self, embedding_service: EmbeddingService):
        self.embedding_service = embedding_service
        self.client = AsyncQdrantClient(
            host=os.getenv("QDRANT_HOST"),
            api_key=os.getenv("QDRANT_API_KEY"),
        )
        self.collection_name = os.getenv("QDRANT_KNOWLEDGE_COLLECTION_NAME", "knowledge_embeddings")

    async def async_init(self):
        await self._create_collection_if_not_exists()

    async def _create_collection_if_not_exists(self):
        try:
            await self.client.get_collection(collection_name=self.collection_name)
            logger.info(f"Collection '{self.collection_name}' already exists.")
        except Exception as e:
            if "Not found" in str(e):  # Specific check for collection not found
                logger.info(f"Collection '{self.collection_name}' not found. Creating it...")
                await self.client.create_collection(
                    collection_name=self.collection_name,
                    vectors_config=qdrant_models.VectorParams(
                        size=TEXT_EMBEDDING_DIMENSIONS,
                        distance=qdrant_models.Distance.COSINE
                    ),
                )
                logger.info(f"Collection '{self.collection_name}' created successfully.")
            else:
                logger.error(f"Error checking or creating collection '{self.collection_name}': {e}")
                raise  # Re-raise other unexpected exceptions
        # Create payload index for family_id, entity_id, and type for efficient filtering
        await self.client.create_payload_index(
            collection_name=self.collection_name,
            field_name="family_id",
            field_schema=qdrant_models.PayloadSchemaType.KEYWORD
        )
        await self.client.create_payload_index(
            collection_name=self.collection_name,
            field_name="entity_id",
            field_schema=qdrant_models.PayloadSchemaType.KEYWORD
        )
        await self.client.create_payload_index(
            collection_name=self.collection_name,
            field_name="type",
            field_schema=qdrant_models.PayloadSchemaType.KEYWORD
        )
        await self.client.create_payload_index(
            collection_name=self.collection_name,
            field_name="visibility",
            field_schema=qdrant_models.PayloadSchemaType.KEYWORD
        )
        logger.info(f"Payload indexes for 'family_id', 'entity_id', 'type', 'visibility' ensured in collection '{self.collection_name}'.")

    async def add_vectors(self, vectors_data: List[VectorData]):
        if not vectors_data:
            logger.warning("No vector data provided to add.")
            return

        summaries = [v_data.summary for v_data in vectors_data]
        # Embed all summaries in a single batch call
        embedded_vectors = self.embedding_service.embed_documents(summaries)

        if len(embedded_vectors) != len(vectors_data):
            logger.error("Mismatch between number of summaries and embedded vectors.")
            raise ValueError("Embedding failed for some documents.")

        points = []
        for i, v_data in enumerate(vectors_data):
            item = v_data.model_dump(exclude_none=True)
            vector = embedded_vectors[i]  # Get the pre-computed embedding

            # Use a unique, stable UUID for the point, derived from family_id and entity_id.
            point_id = str(uuid.uuid5(uuid.NAMESPACE_URL, f"{item['family_id']}-{item['entity_id']}"))

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
                qdrant_models.PointStruct(
                    id=point_id,
                    vector=vector,
                    payload=payload,
                )
            )

        await self.client.upsert(
            collection_name=self.collection_name,
            wait=True,
            points=points
        )
        logger.info(f"Added {len(vectors_data)} vectors to collection '{self.collection_name}'.")

    async def update_vectors(self, update_request: UpdateVectorRequest):
        # Use a unique, stable UUID for the point, derived from family_id and entity_id.
        point_id = str(uuid.uuid5(uuid.NAMESPACE_URL, f"{update_request.family_id}-{update_request.entity_id}"))

        # Fetch current payload to merge updates
        try:
            retrieved_points = await self.client.retrieve(
                collection_name=self.collection_name,
                ids=[point_id],
                with_payload=True,
                with_vectors=True  # Ensure current vector is fetched
            )
            current_point = retrieved_points.pop()
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
        new_vector = None  # If summary is updated, generate a new vector
        if 'summary' in updates:
            new_vector = self.embedding_service.embed_query(new_payload['summary'])

        await self.client.upsert(
            collection_name=self.collection_name,
            wait=True,
            points=[
                qdrant_models.PointStruct(
                    id=point_id,
                    vector=new_vector if new_vector else current_point.vector,
                    payload=new_payload,
                )
            ]
        )
        logger.info(f"Updated point '{point_id}' in collection '{self.collection_name}'.")

    async def delete_vectors(self, delete_request: DeleteVectorRequest) -> int:
        qdrant_filter_conditions = [
            qdrant_models.FieldCondition(
                key="family_id",
                match=qdrant_models.MatchValue(value=delete_request.family_id)
            )
        ]

        if delete_request.entity_id is not None:
            qdrant_filter_conditions.append(
                qdrant_models.FieldCondition(
                    key="entity_id",
                    match=qdrant_models.MatchValue(value=delete_request.entity_id)
                )
            )
        elif delete_request.type is not None:
            qdrant_filter_conditions.append(
                qdrant_models.FieldCondition(
                    key="type",
                    match=qdrant_models.MatchValue(value=delete_request.type)
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
        response = await self.client.delete(
            collection_name=self.collection_name,
            points_selector=qdrant_models.FilterSelector(
                filter=qdrant_models.Filter(
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
            qdrant_models.FieldCondition(
                key="family_id",
                match=qdrant_models.MatchValue(value=family_id)
            )
        ]
        response = await self.client.delete(
            collection_name=self.collection_name,
            points_selector=qdrant_models.FilterSelector(
                filter=qdrant_models.Filter(
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
            qdrant_models.FieldCondition(
                key="family_id",
                match=qdrant_models.MatchValue(value=rebuild_request.family_id)
            )
        ]
        if rebuild_request.entity_ids:
            qdrant_filter_conditions.append(
                qdrant_models.FieldCondition(
                    key="entity_id",
                    match=qdrant_models.MatchAny(any=rebuild_request.entity_ids)
                )
            )

        # Retrieve points to re-embed
        # This will fetch max 10000 points. If more, pagination is needed.
        points_to_rebuild, _ = await self.client.scroll(
            collection_name=self.collection_name,
            scroll_filter=qdrant_models.Filter(must=qdrant_filter_conditions),
            limit=10000,
            with_payload=True,
            with_vectors=True  # Need current vectors to reconstruct PointStruct
        )

        if not points_to_rebuild:
            logger.warning(f"No entries found for rebuilding with filter {qdrant_filter_conditions}.")
            return

        re_embedded_points = []
        summaries_and_points = [] # Store tuples of (summary, original_point) to maintain mapping
        
        for point in points_to_rebuild:
            summary = point.payload.get("summary")
            if summary:
                summaries_and_points.append((summary, point))
            else:
                logger.warning(f"Point {point.id} has no summary to re-embed. Skipping.")
        
        if not summaries_and_points:
            logger.info("No summaries found to re-embed during rebuild.")
            return

        summaries_only = [s for s, p in summaries_and_points]
        re_embedded_vectors = self.embedding_service.embed_documents(summaries_only)

        if len(re_embedded_vectors) != len(summaries_and_points):
            logger.error("Mismatch between number of summaries and re-embedded vectors during rebuild.")
            raise ValueError("Re-embedding failed for some documents during rebuild.")

        for i, (summary, original_point) in enumerate(summaries_and_points):
            new_vector = re_embedded_vectors[i]
            re_embedded_points.append(
                qdrant_models.PointStruct(
                    id=original_point.id,
                    vector=new_vector,
                    payload=original_point.payload  # Keep existing payload
                )
            )

        if re_embedded_points:
            await self.client.upsert(
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
            qdrant_models.FieldCondition(
                key="family_id",
                match=qdrant_models.MatchValue(value=family_id)
            ),
            qdrant_models.FieldCondition(
                key="visibility",
                match=qdrant_models.MatchAny(any=allowed_visibility)
            )
        ]

        search_result = await self.client.query_points(
            collection_name=self.collection_name,
            query=query_vector,
            query_filter=qdrant_models.Filter(must=qdrant_filter_conditions),
            limit=top_k,
            with_payload=True
        )
        formatted_results = []
        for hit in search_result.points:
            formatted_results.append({
                "metadata": hit.payload,  # Qdrant payload is already the metadata
                "summary": hit.payload.get("summary"),
                "score": hit.score
            })
        return formatted_results
