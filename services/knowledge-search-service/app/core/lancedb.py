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


# Initialize LanceDB service globally
lancedb_service = LanceDBService()
