import lancedb
import pandas as pd
from typing import List, Dict, Any, Optional
from loguru import logger
import pyarrow as pa
import json
from uuid import UUID

from ..config import LANCEDB_PATH
from ..schemas.vectors import (
    VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
)
from ..schemas.lancedb_schemas import KNOWLEDGE_LANCEDB_SCHEMA, FACE_LANCEDB_SCHEMA
from ..core.embeddings import EmbeddingService  # Import the class, not the instance


class LanceDBBaseService:
    def __init__(self):
        logger.info(f"Connecting to LanceDB at: {LANCEDB_PATH}")
        self.db = lancedb.connect(LANCEDB_PATH)
        logger.info("Connected to LanceDB.")

    async def _create_table_if_not_exists(self, table_name: str, expected_schema: pa.Schema):
        """
        Ensures a LanceDB table exists with the given schema.
        If not, it creates a new one. If it exists but has an incompatible
        schema (e.g., different vector dimension), it drops and recreates it.
        """
        if table_name not in self.db.table_names():
            logger.info(f"Table '{table_name}' does not exist. Creating it.")
            new_table = self.db.create_table(table_name, schema=expected_schema)
            new_schema = new_table.schema
            new_vector_dim = None
            if 'vector' in new_schema.names and new_schema.field('vector').type.list_size is not None:
                new_vector_dim = new_schema.field('vector').type.list_size
            logger.info(f"Table '{table_name}' created successfully with schema: "
                        f"vector_dim={new_vector_dim}, "
                        f"fields={new_schema.names}.")
        else:
            logger.info(f"Table '{table_name}' already exists. Checking schema.")
            table = self.db.open_table(table_name)
            current_schema = table.schema

            # Log current table info
            current_vector_dim = None
            if 'vector' in current_schema.names and current_schema.field('vector').type.list_size is not None:
                current_vector_dim = current_schema.field('vector').type.list_size
            logger.info(f"Existing table '{table_name}' has schema: "
                        f"vector_dim={current_vector_dim}, "
                        f"fields={current_schema.names}")
            current_vector_dim = None
            expected_vector_dim = None

            if 'vector' in current_schema.names and current_schema.field('vector').type.list_size is not None:
                current_vector_dim = current_schema.field('vector').type.list_size

            if 'vector' in expected_schema.names and expected_schema.field('vector').type.list_size is not None:
                expected_vector_dim = expected_schema.field('vector').type.list_size

            if current_vector_dim is not None and expected_vector_dim is not None and current_vector_dim != expected_vector_dim:
                logger.warning(
                    f"Table '{table_name}' exists but its vector dimension "
                    f"({current_vector_dim}) does not match the expected "
                    f"dimension ({expected_vector_dim}). Dropping and "
                    "recreating the table."
                )
                self.db.drop_table(table_name)
                recreated_table = self.db.create_table(table_name, schema=expected_schema)
                recreated_schema = recreated_table.schema
                recreated_vector_dim = None
                if 'vector' in recreated_schema.names and recreated_schema.field('vector').type.list_size is not None:
                    recreated_vector_dim = recreated_schema.field('vector').type.list_size
                logger.info(f"Table '{table_name}' recreated with updated schema: "
                            f"vector_dim={recreated_vector_dim}, "
                            f"fields={recreated_schema.names}.")
            else:
                logger.info(f"Table '{table_name}' already exists and has "
                            "a compatible schema. No action needed.")


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

        # Perform delete and re-add for simplicity and correctness of vector        # updates.
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
        # Ensure the Face LanceDB schema is correctly applied on service initialization
        # Use a dummy family_id to trigger schema check/creation
        self._create_table_if_not_exists(self._get_face_table_name("temp_init_family_id"), FACE_LANCEDB_SCHEMA)

    def _get_face_table_name(self, family_id: str) -> str:
        return f"faces_{family_id}"

    async def add_face_data(self, family_id: str, faces_data: List[Dict]) -> None:
        if not faces_data:
            logger.warning("No face data provided to add.")
            return
        table_name = self._get_face_table_name(family_id)

        
        table = self.db.open_table(table_name)
        processed_faces = []
        for face in faces_data:
            face_copy = face.copy()  # Work on a copy to avoid modifying original dict during iteration
            # Chuyển đổi UUID sang string
            if "member_id" in face_copy:
                face_copy["member_id"] = str(face_copy["member_id"])
            if "face_id" in face_copy:
                face_copy["face_id"] = str(face_copy["face_id"])
            if "family_id" in face_copy: # Add this line
                face_copy["family_id"] = str(face_copy["family_id"]) # Add this line
            if face_copy.get("bounding_box"):
                face_copy["bounding_box"] = json.dumps(face_copy["bounding_box"])  # Chuyển BoundingBox thành JSON string

            # Đảm bảo có embedding
            if not face_copy.get("embedding"):
                raise ValueError("Embedding must be provided for face data.")
            face_copy["vector"] = face_copy["embedding"]
            # Remove embedding field as it's now 'vector'
            del face_copy["embedding"]
            processed_faces.append(face_copy)
            logger.debug(f"Adding face with face_id: {face_copy.get('face_id')}, member_id: {face_copy.get('member_id')}, vector_db_id: {face_copy.get('vector_db_id')} to processed_faces.")
        
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
        
        # Select all necessary columns to be returned, excluding _distance as it's a meta-column
        results = query.select([
            "family_id", "face_id", "member_id",
            "bounding_box", "confidence", "thumbnail_url", "original_image_url",
            "emotion", "emotion_confidence", "vector_db_id", "is_vector_db_synced"
        ]).limit(top_k).to_list()
        
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

            # Try converting member_id to UUID, log error if fails            # Try converting member_id to UUID, log error if fails            
            member_uuid = None
            member_id_str = res.get("member_id")
            if member_id_str:
                try:
                    member_uuid = UUID(member_id_str)
                except ValueError as e:
                    logger.error(f"Error converting member_id '{member_id_str}' to UUID: {e}")

            # Try converting family_id to UUID
            family_uuid = None
            family_id_str = res.get("family_id")
            if family_id_str:
                try:
                    family_uuid = UUID(family_id_str)
                except ValueError as e:
                    logger.error(f"Error converting family_id '{family_id_str}' to UUID: {e}")

            formatted_results.append({
                "face_id": face_uuid,
                "member_id": member_uuid,
                "family_id": family_uuid, # Add family_id here
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
            logger.debug(f"Retrieved face from LanceDB with face_id: {face_uuid}, member_id: {member_uuid}, vector_db_id: {res.get('vector_db_id')}")        
        return formatted_results

    async def delete_face_data(self, family_id: str, face_id: Optional[str] = None, member_id: Optional[str] = None) -> int:
        """
        Deletes face entries from the specified LanceDB face table.
        """
        table_name = self._get_face_table_name(family_id)
        if table_name not in self.db.table_names():
            logger.warning(f"Face table '{table_name}' does not exist for deletion.")
            return 0
        
        table = self.db.open_table(table_name)
        
        filter_parts = [f"family_id = '{family_id}'"]
        if face_id:
            filter_parts.append(f"face_id = '{face_id}'")
        if member_id:
            filter_parts.append(f"member_id = '{member_id}'")
            
        filter_str = " AND ".join(filter_parts)
        
        if len(filter_parts) == 1 and filter_parts[0] == f"family_id = '{family_id}'":
            logger.warning("No specific face_id or member_id provided for deletion. Deleting all entries for family_id.")
            
        deleted_count = table.delete(where=filter_str)
        logger.info(f"Deleted {deleted_count} face entries from table '{table_name}' with filter '{filter_str}'.")
        return deleted_count

        logger.info(f"Deleted {deleted_count} face entries from table '{table_name}' with filter '{filter_str}'.")
        return deleted_count

    async def delete_faces_by_family_id(self, family_id: str) -> None:
        """
        Deletes all face entries for a given family by dropping the LanceDB table.
        """
        table_name = self._get_face_table_name(family_id)
        if table_name in self.db.table_names():
            logger.info(f"Dropping LanceDB table '{table_name}' for family_id '{family_id}'.")
            self.db.drop_table(table_name)
        else:
            logger.warning(f"Face table '{table_name}' does not exist for family_id '{family_id}'. No action needed.")

    async def update_face_data(self, family_id: str, face_id: str, update_info: Dict) -> int:
        """
        Updates existing face entries in the specified LanceDB face table.
        """
        table_name = self._get_face_table_name(family_id)
        if table_name not in self.db.table_names():
            logger.warning(f"Face table '{table_name}' does not exist for update.")
            return 0
        
        table = self.db.open_table(table_name)
        
        filter_str = f"family_id = '{family_id}' AND face_id = '{face_id}'"
        
        updates = update_info.copy()
        
        # Handle special fields that need serialization or specific processing
        if "bounding_box" in updates and updates["bounding_box"] is not None:
            updates["bounding_box"] = json.dumps(updates["bounding_box"])
        
        # If embedding is updated, it should become the 'vector'
        if "embedding" in updates and updates["embedding"] is not None:
            updates["vector"] = updates["embedding"]
            del updates["embedding"] # Remove original embedding field
            
        if not updates:
            logger.warning("No update fields provided. Nothing to update.")
            return 0
            
        updated_count = table.update(where=filter_str, changes=updates)
        logger.info(f"Updated {updated_count} face entries in table '{table_name}' with filter '{filter_str}'.")
        return updated_count

# Initialize LanceDB service instances globally
# This will be replaced by dependency injection later
from .embeddings import embedding_service as global_embedding_service  # Use the global instance for now

knowledge_lancedb_service = KnowledgeLanceDBService(global_embedding_service)
face_lancedb_service = FaceLanceDBService()
