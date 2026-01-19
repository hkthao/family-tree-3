import lancedb
import pandas as pd
from typing import List, Dict, Any, Optional
from loguru import logger
import pyarrow as pa
import json
from uuid import UUID

from ..config import LANCEDB_PATH, TEXT_EMBEDDING_DIMENSIONS, FACE_EMBEDDING_DIMENSIONS
from ..schemas.vectors import (
    VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
)
from ..schemas.lancedb_schemas import KNOWLEDGE_LANCEDB_SCHEMA, FACE_LANCEDB_SCHEMA
from ..core.embeddings import EmbeddingService # Import the class, not the instance


class LanceDBBaseService:
    def __init__(self):
        logger.info(f"Connecting to LanceDB at: {LANCEDB_PATH}")
        self.db = lancedb.connect(LANCEDB_PATH)
        logger.info("Connected to LanceDB.")

    def _create_table_if_not_exists(self, table_name: str, schema: pa.Schema):
        """
        Ensures a LanceDB table exists with the given schema.
        If not, it creates a new one.
        """
        if table_name not in self.db.table_names():
            logger.info(f"Table '{table_name}' does not exist. Creating it.")
            self.db.create_table(table_name, schema=schema)
            logger.info(f"Table '{table_name}' created successfully.")


class KnowledgeLanceDBService(LanceDBBaseService):
    def __init__(self, embedding_service: EmbeddingService):
        super().__init__()
        self.embedding_service = embedding_service

    def _get_knowledge_table_name(self, family_id: str) -> str:
        return f"family_knowledge_{family_id}"

    def create_dummy_table(self, family_id: str):
        """
        Creates a dummy table for testing and demonstration purposes.
        This method should ideally be called once for setup.
        """
        table_name = self._get_knowledge_table_name(family_id)
        if table_name in self.db.table_names():
            logger.info(f"Dummy table '{table_name}' already exists. "
                        "Deleting and recreating.")
            self.db.drop_table(table_name)

        logger.info(f"Creating dummy table '{table_name}' for family_id: "
                    f"{family_id}")

        # Dummy data (now with full metadata)
        dummy_metadata_family = {
            "family_id": family_id,
            "original_id": family_id,
            "content_type": "family",
            "name": "Dòng họ Nguyễn Văn (Demo)",
            "visibility": "public",
            "code": "NGUYEN-DEMO",
            "description": ("Dòng họ có truyền thống lâu đời tại Việt Nam, "
                            "nổi tiếng với nhiều nhân vật lịch sử và đóng "
                            "góp cho cộng đồng.")
        }
        dummy_metadata_member1 = {
            "family_id": family_id,
            "original_id": "M001",
            "content_type": "member",
            "name": "Nguyễn Văn A",
            "visibility": "public",
            "code": "M001-CODE",
            "full_name": "Nguyễn Văn A",
            "gender": "Male",
            "biography": ("Ông tổ đời thứ 3 của dòng họ Nguyễn, có công "
                          "khai phá vùng đất mới.")
        }
        dummy_metadata_member2 = {
            "family_id": family_id,
            "original_id": "M002",
            "content_type": "member",
            "name": "Trần Thị B",
            "visibility": "private",
            "code": "M002-CODE",
            "full_name": "Trần Thị B",
            "gender": "Female",
            "biography": ("Vợ của Nguyễn Văn A, người phụ nữ hiền thục, "
                          "đảm đang.")
        }

        data_to_embed = [
            {
                "family_id": family_id,
                "entity_id": family_id,  # family id and entity id are same
                                         # for family root
                "type": "family",
                "visibility": "public",
                "name": "Dòng họ Nguyễn Văn (Demo)",
                "summary": ("Dòng họ có truyền thống lâu đời tại Việt Nam, "
                            "nổi tiếng với nhiều nhân vật lịch sử và đóng "
                            "góp cho cộng đồng."),
                "metadata": dummy_metadata_family
            },
            {
                "family_id": family_id,
                "entity_id": "M001",
                "type": "member",
                "visibility": "public",
                "name": "Nguyễn Văn A",
                "summary": ("Ông tổ đời thứ 3 của dòng họ Nguyễn, có công "
                            "khai phá vùng đất mới."),
                "metadata": dummy_metadata_member1
            },
            {
                "family_id": family_id,
                "entity_id": "M002",
                "type": "member",
                "visibility": "private",
                "name": "Trần Thị B",
                "summary": "Vợ của Nguyễn Văn A, người phụ nữ hiền thục, "
                           "đảm đang.",
                "metadata": dummy_metadata_member2
            },
        ]

        # Generate embeddings for each entry and serialize metadata
        processed_data = []
        for item in data_to_embed:
            item["vector"] = self.embedding_service.embed_query(item["summary"])
            # Serialize metadata to JSON string
            item["metadata"] = json.dumps(item["metadata"])
            processed_data.append(item)

        df = pd.DataFrame(processed_data)
        self.db.create_table(table_name, data=df, schema=KNOWLEDGE_LANCEDB_SCHEMA)
        logger.info(f"Dummy table '{table_name}' created with "
                    f"{len(processed_data)} entries.")

    def add_vectors(self, table_name: str, vectors_data: List[VectorData]):
        """
        Adds new vector entries to the specified LanceDB table by generating
        embeddings from summaries.
        """
        if not vectors_data:
            logger.warning("No vector data provided to add.")
            return

        # Ensure the table exists
        self._create_table_if_not_exists(table_name, KNOWLEDGE_LANCEDB_SCHEMA)

        processed_data = []
        for v_data in vectors_data:
            item = v_data.model_dump(exclude_none=True)
            item["vector"] = self.embedding_service.embed_query(item["summary"])
            # Serialize metadata from Pydantic model to JSON string for
            # LanceDB storage
            item["metadata"] = json.dumps(v_data.metadata)
            processed_data.append(item)

        df = pd.DataFrame(processed_data)

        table = self.db.open_table(table_name)
        table.add(df)
        logger.info(f"Added {len(vectors_data)} vectors to table "
                    f"'{table_name}'.")

    def update_vectors(self, table_name: str, update_request: UpdateVectorRequest):
        """
        Updates existing vector entries in the specified LanceDB table.
        """
        filter_str = f"family_id = '{update_request.family_id}'"
        if update_request.entity_id is not None:
            filter_str += f" AND entity_id = '{update_request.entity_id}'"

        updates = update_request.model_dump(exclude_unset=True,
                                            exclude={'family_id', 'entity_id'})

        if not updates:
            logger.warning("No update fields provided. Nothing to update.")
            return

        # If summary is updated, generate a new vector
        if 'summary' in updates:
            updates['vector'] = self.embedding_service.embed_query(
                updates['summary']
            )
            logger.info(f"Generated new embedding for updated summary in "
                        f"table '{table_name}'.")

        # Serialize metadata if present in updates
        if 'metadata' in updates and updates['metadata'] is not None:
            updates['metadata'] = json.dumps(updates['metadata'])

        table = self.db.open_table(table_name)

        if updates:  # Ensure there are actual updates after vector generation
            logger.info(f"Updating data for table '{table_name}' with filter "
                        f"'{filter_str}'. Changes: {updates}")
            # LanceDB's update can take a dictionary of changes and apply them
            # directly
            table.update(where=filter_str, changes=updates)

        logger.info(f"Finished update operation for table '{table_name}' "
                    f"with filter '{filter_str}'.")

    def delete_vectors(self, table_name: str, delete_request: DeleteVectorRequest) -> int:
        """
        Deletes vector entries from the specified LanceDB table based on
        family_id and optionally entity_id, type, or a custom where_clause.
        Returns the number of deleted rows.
        """
        # Ensure the table exists before trying to delete
        self._create_table_if_not_exists(table_name, KNOWLEDGE_LANCEDB_SCHEMA)
        
        table = self.db.open_table(table_name)
        
        if delete_request.where_clause:
            filter_str = delete_request.where_clause
            logger.info(f"Deleting vectors from table '{table_name}' using "
                        f"custom where_clause: '{filter_str}'.")
        else:
            filter_str = f"family_id = '{delete_request.family_id}'"

            if delete_request.entity_id is not None:
                filter_str += f" AND entity_id = '{delete_request.entity_id}'"
            elif delete_request.type is not None:
                filter_str += f" AND type = '{delete_request.type}'"
            else:
                logger.warning("No specific entity_id or type provided for "
                               "deletion. Deleting all entries for family_id: "
                               f"{delete_request.family_id}")
        
        # LanceDB delete method returns the number of rows deleted.
        deleted_count = table.delete(where=filter_str)
        logger.info(f"Deleted {deleted_count} vectors from table '{table_name}' with filter "
                    f"'{filter_str}'.")
        return deleted_count


    def rebuild_vectors(self, rebuild_request: RebuildVectorRequest):
        """
        Rebuilds vectors for the specified family_id and optional entity_ids
        by re-embedding their summaries.
        """
        table_name = self._get_knowledge_table_name(rebuild_request.family_id)
        if table_name not in self.db.table_names():
            logger.warning(f"Table '{table_name}' does not exist for "
                           "rebuilding. Creating dummy table.")
            self.create_dummy_table(rebuild_request.family_id)

        table = self.db.open_table(table_name)

        filter_str = f"family_id = '{rebuild_request.family_id}'"
        if rebuild_request.entity_ids:
            # Build an 'IN' clause for entity_ids
            entity_ids_str = ", ".join([f"'{eid}'"
                                        for eid in rebuild_request.entity_ids])
            filter_str += f" AND entity_id IN ({entity_ids_str})"
            logger.info(f"Rebuilding vectors for specific entities in table "
                        f"'{table_name}' with filter '{filter_str}'.")
        else:
            logger.info(f"Rebuilding all vectors for table '{table_name}'.")

        # Fetch existing data that needs to be rebuilt
        current_data = table.query().select(["entity_id", "summary", "type",
                                             "visibility", "name",
                                             "family_id", "metadata"]).where(
                                                 filter_str).to_list()

        if not current_data:
            logger.warning(f"No entries found for rebuilding with filter "
                           f"'{filter_str}'.")
            return

        re_embedded_data_for_add = []
        for item in current_data:
            new_vector = self.embedding_service.embed_query(item["summary"])
            re_embedded_data_for_add.append({
                "entity_id": item["entity_id"],
                "family_id": item["family_id"],
                "type": item["type"],
                "visibility": item["visibility"],
                "name": item["name"],
                "summary": item["summary"],
                "vector": new_vector,
                "metadata": item["metadata"]  # Pass original serialized metadata
            })

        # Perform delete and re-add for simplicity and correctness of vector
        # updates.
        logger.info(f"Deleting existing entries for rebuild filter: "
                    f"'{filter_str}'.")
        table.delete(where=filter_str)
        logger.info(f"Adding re-embedded entries to table '{table_name}'.")

        # Convert re_embedded_data_for_add back to DataFrame for adding
        df_re_embedded = pd.DataFrame(re_embedded_data_for_add)
        table.add(df_re_embedded)

        logger.info(f"Rebuild process for table '{table_name}' completed for "
                    f"{len(re_embedded_data_for_add)} entries.")

    def search_knowledge_table(
        self,
        family_id: str,
        query_vector: List[float],
        allowed_visibility: List[str],
        top_k: int
    ) -> List[Dict[str, Any]]:
        table_name = self._get_knowledge_table_name(family_id)

        if table_name not in self.db.table_names():
            logger.warning(
                f"Family knowledge table '{table_name}' does not exist. "
                "Returning empty results."
            )
            return []

        try:
            table = self.db.open_table(table_name)

            # Construct the visibility filter
            visibility_filter = " OR ".join([f"visibility = '{v}'"
                                             for v in allowed_visibility])

            results = (
                table.search(query_vector)
                .where(visibility_filter)
                .limit(top_k)
                .to_list()
            )

            # Format results
            formatted_results = []
            for res in results:
                # Deserialize metadata back to dictionary
                metadata = json.loads(res.get("metadata", "{}"))
                formatted_results.append({
                    "metadata": metadata,  # Return the full metadata dictionary
                    "summary": res.get("summary"),
                    # LanceDB returns _distance as score
                    "score": res.get("_distance")
                })
            return formatted_results

        except Exception as e:
            logger.error(f"Error searching LanceDB knowledge table '{table_name}': {e}")
            return []


class FaceLanceDBService(LanceDBBaseService):
    def __init__(self):
        super().__init__()

    def _get_face_table_name(self, family_id: str) -> str:
        return f"faces_{family_id}"

    async def add_face_data(self, family_id: str, faces_data: List[Dict]) -> None:
        if not faces_data:
            logger.warning("No face data provided to add.")
            return

        table_name = self._get_face_table_name(family_id)
        self._create_table_if_not_exists(table_name, FACE_LANCEDB_SCHEMA)
        
        table = self.db.open_table(table_name)

        processed_faces = []
        for face in faces_data:
            face_copy = face.copy() # Work on a copy to avoid modifying original dict during iteration
            # Chuyển đổi UUID sang string
            if "member_id" in face_copy:
                face_copy["member_id"] = str(face_copy["member_id"])
            if "face_id" in face_copy:
                face_copy["face_id"] = str(face_copy["face_id"])
            if face_copy.get("bounding_box"):
                face_copy["bounding_box"] = json.dumps(face_copy["bounding_box"]) # Chuyển BoundingBox thành JSON string
            
            # Đảm bảo có embedding
            if not face_copy.get("embedding"):
                raise ValueError("Embedding must be provided for face data.")
            face_copy["vector"] = face_copy["embedding"]
            # Remove embedding field as it's now 'vector'
            del face_copy["embedding"]
            
            processed_faces.append(face_copy)
        
        df = pd.DataFrame(processed_faces)
        table.add(df)
        logger.info(f"Added {len(faces_data)} face entries to table '{table_name}'.")

    async def search_faces(self, family_id: str, query_embedding: List[float], member_id: Optional[str] = None, top_k: int = 5) -> List[Dict[str, Any]]:
        if not query_embedding:
            raise ValueError("Query embedding must be provided for face search.")
        
        table_name = self._get_face_table_name(family_id)
        if table_name not in self.db.table_names():
            logger.warning(
                f"Face table '{table_name}' does not exist. "
                "Returning empty results."
            )
            return []

        table = self.db.open_table(table_name)
        
        face_filter = None
        if member_id:
            face_filter = f"member_id = '{member_id}'"
        
        query = table.search(query_embedding)
        if face_filter:
            query = query.where(face_filter)
        
        results = query.limit(top_k).to_list()
        
        # Format results
        formatted_results = []
        for res in results:
            # Deserialize bounding_box
            if res.get("bounding_box"):
                res["bounding_box"] = json.loads(res["bounding_box"])
            
            # Try converting face_id to UUID, log error if fails
            face_uuid = None
            face_id_str = res.get("face_id")
            if face_id_str:
                try:
                    face_uuid = UUID(face_id_str)
                except ValueError as e:
                    logger.error(f"Error converting face_id '{face_id_str}' to UUID: {e}")

            # Try converting member_id to UUID, log error if fails
            member_uuid = None
            member_id_str = res.get("member_id")
            if member_id_str:
                try:
                    member_uuid = UUID(member_id_str)
                except ValueError as e:
                    logger.error(f"Error converting member_id '{member_id_str}' to UUID: {e}")

            formatted_results.append({
                "face_id": face_uuid,
                "member_id": member_uuid,
                "score": res.get("_distance"),  # LanceDB returns _distance as score
                "bounding_box": res.get("bounding_box"),
                "confidence": res.get("confidence"),
                "thumbnail_url": res.get("thumbnail_url"),
                "original_image_url": res.get("original_image_url"),
                "emotion": res.get("emotion"),
                "emotion_confidence": res.get("emotion_confidence"),
                "vector_db_id": res.get("vector_db_id"),
                "is_vector_db_synced": res.get("is_vector_db_synced")
            })
        
        return formatted_results


    async def delete_face_data(self, family_id: str, face_id: Optional[str] = None, member_id: Optional[str] = None) -> int:
        if not family_id:
            raise ValueError("family_id must be provided for face deletion.")
        
        table_name = self._get_face_table_name(family_id)
        if table_name not in self.db.table_names():
            logger.warning(f"Face table '{table_name}' does not exist. Nothing to delete.")
            return 0
        
        table = self.db.open_table(table_name)
        
        filter_str = ""
        if face_id:
            filter_str = f"face_id = '{face_id}'"
        elif member_id:
            filter_str = f"member_id = '{member_id}'"
        else:
            raise ValueError("Either face_id or member_id must be provided for deletion.")
        
        deleted_count = table.delete(where=filter_str)
        
        logger.info(f"Deleted {deleted_count} entries from face table "
                    f"'{table_name}' with filter '{filter_str}'.")
        return deleted_count

    async def update_face_data(self, family_id: str, face_id: str, update_data: Dict) -> int:
        if not family_id:
            raise ValueError("family_id must be provided for face update.")
        
        table_name = self._get_face_table_name(family_id)
        if table_name not in self.db.table_names():
            logger.warning(f"Face table '{table_name}' does not exist. Nothing to update.")
            return 0
        
        table = self.db.open_table(table_name)

        filter_str = f"face_id = '{face_id}'"
        
        processed_update_data = {}
        for key, value in update_data.items():
            if key == "bounding_box" and value is not None:
                processed_update_data[key] = json.dumps(value)
            elif key == "member_id" and value is not None:
                processed_update_data[key] = str(value)
            elif key == "face_id" and value is not None:
                processed_update_data[key] = str(value)
            elif key == "embedding" and value is not None: # Update vector if embedding is provided
                processed_update_data["vector"] = value
            else:
                processed_update_data[key] = value

        if not processed_update_data:
            logger.warning("No valid update fields provided. Nothing to update.")
            return 0

        updated_count = table.update(where=filter_str, **processed_update_data)
        
        if updated_count > 0:
            logger.info(f"Updated {updated_count} entries for face_id '{face_id}' in table "
                        f"'{table_name}'. Changes: {processed_update_data}")
        else:
            logger.info(f"No entries found to update for face_id '{face_id}' in table "
                        f"'{table_name}'.")
        return updated_count


# Initialize LanceDB service instances globally
# This will be replaced by dependency injection later
from .embeddings import embedding_service as global_embedding_service # Use the global instance for now

knowledge_lancedb_service = KnowledgeLanceDBService(global_embedding_service)
face_lancedb_service = FaceLanceDBService()

