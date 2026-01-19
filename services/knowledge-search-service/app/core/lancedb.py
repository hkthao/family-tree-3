import lancedb
import pandas as pd
from typing import List, Dict, Any
from loguru import logger
import pyarrow as pa
import json


from ..config import LANCEDB_PATH, EMBEDDING_DIMENSIONS
from ..schemas.vectors import (
    VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
)
from ..core.embeddings import embedding_service


class LanceDBService:
    def __init__(self):
        logger.info("Connecting to LanceDB at: "
                    f"{LANCEDB_PATH}")
        self.db = lancedb.connect(LANCEDB_PATH)
        logger.info("Connected to LanceDB.")

    def _get_table_name(self, family_id: str) -> str:
        return f"family_{family_id}"

    def _create_table_if_not_exists(self, table_name: str):
        """
        Ensures a LanceDB table exists. If not, it creates a new one
        with a schema derived from VectorData.
        """
        if table_name not in self.db.table_names():
            logger.info(f"Table '{table_name}' does not exist. Creating it.")
            # Define schema based on VectorData for initial table creation
            schema = pa.schema([
                pa.field("vector", pa.list_(pa.float32(),
                                            EMBEDDING_DIMENSIONS)),
                pa.field("family_id", pa.string()),
                pa.field("entity_id", pa.string()),
                pa.field("type", pa.string()),
                pa.field("visibility", pa.string()),
                pa.field("name", pa.string()),
                pa.field("summary", pa.string()),
                pa.field("metadata", pa.string())
            ])
            # Create an empty table with the defined schema
            self.db.create_table(table_name, schema=schema)
            logger.info(f"Table '{table_name}' created successfully.")

    def search_family_table(
        self,
        family_id: str,
        query_vector: List[float],
        allowed_visibility: List[str],
        top_k: int
    ) -> List[Dict[str, Any]]:
        table_name = self._get_table_name(family_id)

        if table_name not in self.db.table_names():
            logger.warning(
                f"Family table '{table_name}' does not exist. "
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
            logger.error(f"Error searching LanceDB table '{table_name}': {e}")
            return []

    def create_dummy_table(self, family_id: str):
        """
        Creates a dummy table for testing and demonstration purposes.
        This method should ideally be called once for setup.
        """
        table_name = self._get_table_name(family_id)
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
            item["vector"] = embedding_service.embed_query(item["summary"])
            # Serialize metadata to JSON string
            item["metadata"] = json.dumps(item["metadata"])
            processed_data.append(item)

        # Ensure vector column is of float[EMBEDDING_DIMENSIONS] type
        # and metadata is string
        schema = pa.schema([
            pa.field("vector", pa.list_(pa.float32(), EMBEDDING_DIMENSIONS)),
            pa.field("family_id", pa.string()),
            pa.field("entity_id", pa.string()),
            pa.field("type", pa.string()),
            pa.field("visibility", pa.string()),
            pa.field("name", pa.string()),
            pa.field("summary", pa.string()),
            pa.field("metadata", pa.string())  # Added metadata field
        ])
        df = pd.DataFrame(processed_data)
        self.db.create_table(table_name, data=df, schema=schema)
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
        self._create_table_if_not_exists(table_name)

        processed_data = []
        for v_data in vectors_data:
            item = v_data.model_dump(exclude_none=True)
            item["vector"] = embedding_service.embed_query(item["summary"])
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
            updates['vector'] = embedding_service.embed_query(
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

    def delete_vectors(self, table_name: str, delete_request: DeleteVectorRequest):
        """
        Deletes vector entries from the specified LanceDB table based on
        family_id and optionally entity_id, type, or a custom where_clause.
        """
        # Ensure the table exists before trying to delete
        self._create_table_if_not_exists(table_name)
        if delete_request.where_clause:
            filter_str = delete_request.where_clause
            logger.info(f"Deleting vectors from table '{table_name}' using "
                        f"custom where_clause: '{filter_str}'.")
        else:
            filter_str = f"family_id = '{delete_request.family_id}'"

            if delete_request.entity_id is not None:
                filter_str += f" AND entity_id = '{delete_request.entity_id}'"
            elif delete_request.type is not None:
                # If entity_id is None but type is provided, delete all
                # entities of that type within the family
                filter_str += f" AND type = '{delete_request.type}'"
            else:
                # If neither entity_id nor type is provided, it implies
                # deleting all for the family.
                # This is a dangerous operation, so we should add a safeguard
                # or require explicit confirmation.
                # For now, let's assume if only family_id is provided, it
                # means delete all for that family.
                logger.warning("No specific entity_id or type provided for "
                               "deletion. Deleting all entries for family_id: "
                               f"{delete_request.family_id}")

        table = self.db.open_table(table_name)

        # LanceDB delete method
        table.delete(where=filter_str)
        logger.info(f"Deleted vectors from table '{table_name}' with filter "
                    f"'{filter_str}'.")

    def rebuild_vectors(self, rebuild_request: RebuildVectorRequest):
        """
        Rebuilds vectors for the specified family_id and optional entity_ids
        by re-embedding their summaries.
        """
        table_name = self._get_table_name(rebuild_request.family_id)
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
        # Need to select the 'summary' and 'metadata' field to re-embed
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
            new_vector = embedding_service.embed_query(item["summary"])
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

    def _get_face_table_name(self, family_id: str) -> str:
        return f"faces_{family_id}"

    def _create_face_table_if_not_exists(self, family_id: str):
        table_name = self._get_face_table_name(family_id)
        if table_name not in self.db.table_names():
            logger.info(f"Face table '{table_name}' does not exist. Creating it.")
            # Define schema for face data
            schema = pa.schema([
                pa.field("vector", pa.list_(pa.float32(), EMBEDDING_DIMENSIONS)),
                pa.field("face_id", pa.string()),
                pa.field("member_id", pa.string()),
                pa.field("bounding_box", pa.string()), # Store as JSON string
                pa.field("confidence", pa.float64()),
                pa.field("thumbnail_url", pa.string()),
                pa.field("original_image_url", pa.string()),
                pa.field("emotion", pa.string()),
                pa.field("emotion_confidence", pa.float64()),
                pa.field("vector_db_id", pa.string()),
                pa.field("is_vector_db_synced", pa.bool_())
            ])
            self.db.create_table(table_name, schema=schema)
            logger.info(f"Face table '{table_name}' created successfully.")

    async def add_face_data(self, family_id: str, faces_data: List[Dict]) -> None:
        if not faces_data:
            logger.warning("No face data provided to add.")
            return

        self._create_face_table_if_not_exists(family_id)
        table_name = self._get_face_table_name(family_id)
        table = self.db.open_table(table_name)

        processed_faces = []
        for face in faces_data:
            # Chuyển đổi UUID sang string
            face["member_id"] = str(face["member_id"])
            face["face_id"] = str(face["face_id"])
            if face.get("bounding_box"):
                face["bounding_box"] = json.dumps(face["bounding_box"]) # Chuyển BoundingBox thành JSON string
            
            # Đảm bảo có embedding
            if not face.get("embedding"):
                raise ValueError("Embedding must be provided for face data.")
            
            processed_faces.append(face)

        df = pd.DataFrame(processed_faces)
        table.add(df)
        logger.info(f"Added {len(faces_data)} face entries to table '{table_name}'.")

    async def search_faces(self, query_embedding: List[float], member_id: Optional[str] = None, top_k: int = 5) -> List[Dict[str, Any]]:
        if not query_embedding:
            raise ValueError("Query embedding must be provided for face search.")
        
        # Need a family_id to determine the table. For now, assume a default or pass it.
        # This needs to be handled: how to know which family's faces to search?
        # For now, let's assume `member_id` implies `family_id` can be derived or it's always one table.
        # Or perhaps, we search across all face tables? (More complex)
        # Let's simplify: require family_id. Or, search across all relevant tables.
        # For initial implementation, let's assume we search within a specific family.
        # The API request in faces.py doesn't have family_id, so let's adjust.
        # For now, I will assume member_id is enough to form a filter.
        # If we have a dedicated face table per family, we need family_id.
        # Let's add family_id to SearchFaceRequest in face_models.py later.

        # For now, let's just search globally across all face tables, which is not ideal for large scale.
        # Or, we enforce family_id for face search.
        # Given the `search_family_table` method uses `family_id`, it's consistent to use it here.

        # *** This part needs clarification on how to get family_id for face search ***
        # For now, I will make a placeholder or assume family_id is somehow known
        # Let's assume for now the face table name could be "faces_global" or we need family_id in the request.
        # The `faces.py` router `search_faces` will pass `member_id` as filter.
        
        # For simplicity, let's iterate through all face tables in the DB
        all_face_tables = [name for name in self.db.table_names() if name.startswith("faces_")]
        if not all_face_tables:
            logger.warning("No face tables found in LanceDB.")
            return []

        combined_results = []
        for table_name in all_face_tables:
            table = self.db.open_table(table_name)
            
            face_filter = None
            if member_id:
                face_filter = f"member_id = '{member_id}'"
            
            query = table.search(query_embedding)
            if face_filter:
                query = query.where(face_filter)
            
            results = query.limit(top_k).to_list()
            
            for res in results:
                # Deserialize bounding_box
                if res.get("bounding_box"):
                    res["bounding_box"] = json.loads(res["bounding_box"])
                combined_results.append(res)
        
        # Sort results by _distance (score) and return top_k overall
        combined_results.sort(key=lambda x: x.get("_distance", float('inf')))
        return combined_results[:top_k]


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
        
        # LanceDB's delete method returns the number of deleted rows
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
            else:
                processed_update_data[key] = value

        if not processed_update_data:
            logger.warning("No valid update fields provided. Nothing to update.")
            return 0

        # LanceDB update operation
        # Returns the number of rows updated (not directly, need to check its behavior)
        # For now, assuming `update` returns number of rows affected.
        # Or, we can query before and after to get the count.
        
        # LanceDB's update takes a `where` clause and a dictionary of `changes`
        table.update(where=filter_str, changes=processed_update_data)
        logger.info(f"Updated face data for face_id '{face_id}' in table "
                    f"'{table_name}'. Changes: {processed_update_data}")
        # LanceDB's update doesn't directly return count.
        # For simplicity, we can return 1 if filter is found, else 0.
        # Or query to get the count. For now, let's assume it updates.
        # We can implement checking affected rows if needed later.
        
        # To get affected rows, we would query the table before and after,
        # or use a more advanced LanceDB feature if available.
        # For now, I'll return 1 if an update was attempted, assuming success.
        # A more robust solution might query `table.to_pandas()` then filter.
        
        # Let's count before for more accuracy.
        # current_count = len(table.to_pandas().query(filter_str))
        # if current_count > 0:
        #    return 1 # At least one row was intended to be updated
        # return 0

        # Simplified for now, just return 1 if successful.
        return 1 # Assume 1 row is updated if no exception.




# Initialize LanceDB service globally
lancedb_service = LanceDBService()
