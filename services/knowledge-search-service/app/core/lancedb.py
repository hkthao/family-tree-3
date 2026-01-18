import lancedb
import pandas as pd
from typing import List, Dict, Any
from loguru import logger
import numpy as np
import pyarrow as pa

from ..config import LANCEDB_PATH, EMBEDDING_DIMENSIONS
from ..schemas.vectors import VectorData, AddVectorRequest, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
from ..core.embeddings import embedding_service

class LanceDBService:
    def __init__(self):
        logger.info(f"Connecting to LanceDB at: {LANCEDB_PATH}")
        self.db = lancedb.connect(LANCEDB_PATH)
        logger.info("Connected to LanceDB.")

    def _get_table_name(self, family_id: str) -> str:
        return f"family_{family_id}"

    def search_family_table(
        self,
        family_id: str,
        query_vector: List[float],
        allowed_visibility: List[str],
        top_k: int
    ) -> List[Dict[str, Any]]:
        table_name = self._get_table_name(family_id)
        
        if table_name not in self.db.table_names():
            logger.warning(f"Family table '{table_name}' does not exist. Returning empty results.")
            return []

        try:
            table = self.db.open_table(table_name)
            
            # Construct the visibility filter
            visibility_filter = " OR ".join([f"visibility = '{v}'" for v in allowed_visibility])
            
            results = (
                table.search(query_vector)
                .where(visibility_filter)
                .limit(top_k)
                .to_list()
            )
            
            # Format results
            formatted_results = []
            for res in results:
                formatted_results.append({
                    "entity_id": res.get("entity_id"), # Renamed from member_id
                    "type": res.get("type"), # Include the new type field
                    "name": res.get("name"),
                    "summary": res.get("summary"),
                    "score": res.get("_distance") # LanceDB returns _distance as score
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
            logger.info(f"Dummy table '{table_name}' already exists. Deleting and recreating.")
            self.db.drop_table(table_name)
        
        logger.info(f"Creating dummy table '{table_name}' for family_id: {family_id}")

        # Dummy data
        # For 'family' type, entity_id is None, for 'member' type, it's a string
        data_to_embed = [
            {
                "family_id": family_id,
                "entity_id": None,
                "type": "family",
                "visibility": "public",
                "name": "Dòng họ Nguyễn Văn (Demo)",
                "summary": "Dòng họ có truyền thống lâu đời tại Việt Nam, nổi tiếng với nhiều nhân vật lịch sử và đóng góp cho cộng đồng."
            },
            {
                "family_id": family_id,
                "entity_id": "M001",
                "type": "member",
                "visibility": "public",
                "name": "Nguyễn Văn A",
                "summary": "Ông tổ đời thứ 3 của dòng họ Nguyễn, có công khai phá vùng đất mới."
            },
            {
                "family_id": family_id,
                "entity_id": "M002",
                "type": "member",
                "visibility": "private",
                "name": "Trần Thị B",
                "summary": "Vợ của Nguyễn Văn A, người phụ nữ hiền thục, đảm đang."
            },
            {
                "family_id": family_id,
                "entity_id": "M003",
                "type": "member",
                "visibility": "public",
                "name": "Lê Văn C",
                "summary": "Người con trai cả của Nguyễn Văn A, nổi tiếng với tài trí hơn người."
            },
            {
                "family_id": family_id,
                "entity_id": "M004",
                "type": "member",
                "visibility": "deleted", # Should not be searchable
                "name": "Phạm Thị D",
                "summary": "Thông tin thành viên đã bị xóa."
            },
            {
                "family_id": "ANOTHER_FAMILY", # Different family, should not be searchable under F123
                "entity_id": "M005",
                "type": "member",
                "visibility": "public",
                "name": "Nguyễn Văn X",
                "summary": "Thành viên của một dòng họ khác."
            }
        ]
        
        # Generate embeddings for each entry
        processed_data = []
        for item in data_to_embed:
            item["vector"] = embedding_service.embed_query(item["summary"])
            if item["entity_id"] is None:
                item["entity_id"] = "" # LanceDB schema expects string, so use empty string for None
            processed_data.append(item)

        # Ensure vector column is of float[EMBEDDING_DIMENSIONS] type
        schema = pa.schema([
            pa.field("vector", pa.list_(pa.float32(), EMBEDDING_DIMENSIONS)),
            pa.field("family_id", pa.string()),
            pa.field("entity_id", pa.string()), # Renamed from member_id to entity_id
            pa.field("type", pa.string()), # New field for differentiating entity types (e.g., 'family', 'member')
            pa.field("visibility", pa.string()),
            pa.field("name", pa.string()),
            pa.field("summary", pa.string())
        ])
        df = pd.DataFrame(processed_data)
        self.db.create_table(table_name, data=df, schema=schema)
        logger.info(f"Dummy table '{table_name}' created with {len(processed_data)} entries.")

    def add_vectors(self, table_name: str, vectors_data: List[VectorData]):
        """
        Adds new vector entries to the specified LanceDB table by generating embeddings from summaries.
        """
        if not vectors_data:
            logger.warning("No vector data provided to add.")
            return

        processed_data = []
        for v_data in vectors_data:
            item = v_data.model_dump(exclude_none=True)
            item["vector"] = embedding_service.embed_query(item["summary"])
            if item["entity_id"] is None:
                item["entity_id"] = "" # LanceDB schema expects string, so use empty string for None
            processed_data.append(item)

        df = pd.DataFrame(processed_data)
        
        table = self.db.open_table(table_name)
        table.add(df)
        logger.info(f"Added {len(vectors_data)} vectors to table '{table_name}'.")

    def update_vectors(self, table_name: str, update_request: UpdateVectorRequest):
        """
        Updates existing vector entries in the specified LanceDB table.
        """
        filter_str = f"family_id = '{update_request.family_id}'"
        if update_request.entity_id is not None:
            filter_str += f" AND entity_id = '{update_request.entity_id}'"
        
        updates = update_request.model_dump(exclude_unset=True, exclude={'family_id', 'entity_id'})
        
        if not updates:
            logger.warning("No update fields provided. Nothing to update.")
            return

        # If summary is updated, generate a new vector
        if 'summary' in updates:
            updates['vector'] = embedding_service.embed_query(updates['summary'])
            logger.info(f"Generated new embedding for updated summary in table '{table_name}'.")

        table = self.db.open_table(table_name)
        
        if updates: # Ensure there are actual updates after vector generation
            logger.info(f"Updating data for table '{table_name}' with filter '{filter_str}'. Changes: {updates}")
            # LanceDB's update can take a dictionary of changes and apply them directly
            table.update(where=filter_str, changes=updates)
        
        logger.info(f"Finished update operation for table '{table_name}' with filter '{filter_str}'.")
    def delete_vectors(self, table_name: str, delete_request: DeleteVectorRequest):
        """
        Deletes vector entries from the specified LanceDB table based on family_id and optionally entity_id, type, or a custom where_clause.
        """
        if delete_request.where_clause:
            filter_str = delete_request.where_clause
            logger.info(f"Deleting vectors from table '{table_name}' using custom where_clause: '{filter_str}'.")
        else:
            filter_str = f"family_id = '{delete_request.family_id}'"
            
            if delete_request.entity_id is not None:
                filter_str += f" AND entity_id = '{delete_request.entity_id}'"
            elif delete_request.type is not None:
                # If entity_id is None but type is provided, delete all entities of that type within the family
                filter_str += f" AND type = '{delete_request.type}'"
            else:
                # If neither entity_id nor type is provided, it implies deleting all for the family.
                # This is a dangerous operation, so we should add a safeguard or require explicit confirmation.
                # For now, let's assume if only family_id is provided, it means delete all for that family.
                logger.warning(f"No specific entity_id or type provided for deletion. Deleting all entries for family_id: {delete_request.family_id}")

        table = self.db.open_table(table_name)
        
        # LanceDB delete method
        table.delete(where=filter_str)
        logger.info(f"Deleted vectors from table '{table_name}' with filter '{filter_str}'.")

    def rebuild_vectors(self, rebuild_request: RebuildVectorRequest):
        """
        Rebuilds vectors for the specified family_id and optional entity_ids by re-embedding their summaries.
        """
        table_name = self._get_table_name(rebuild_request.family_id)
        if table_name not in self.db.table_names():
            logger.warning(f"Table '{table_name}' does not exist for rebuilding. Creating dummy table.")
            self.create_dummy_table(rebuild_request.family_id) # Recreate if not exists

        table = self.db.open_table(table_name)
        
        filter_str = f"family_id = '{rebuild_request.family_id}'"
        if rebuild_request.entity_ids:
            # Build an 'IN' clause for entity_ids
            entity_ids_str = ", ".join([f"'{eid}'" for eid in rebuild_request.entity_ids])
            filter_str += f" AND entity_id IN ({entity_ids_str})"
            logger.info(f"Rebuilding vectors for specific entities in table '{table_name}' with filter '{filter_str}'.")
        else:
            logger.info(f"Rebuilding all vectors for table '{table_name}'.")

        # Fetch existing data that needs to be rebuilt
        # Need to select the 'summary' field to re-embed
        current_data = table.query().select(["entity_id", "summary", "type", "visibility", "name", "family_id"]).where(filter_str).to_list()

        if not current_data:
            logger.warning(f"No entries found for rebuilding with filter '{filter_str}'.")
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
                "vector": new_vector
            })
        
        # Perform delete and re-add for simplicity and correctness of vector updates.
        logger.info(f"Deleting existing entries for rebuild filter: '{filter_str}'.")
        table.delete(where=filter_str)
        logger.info(f"Adding re-embedded entries to table '{table_name}'.")
        
        # Convert re_embedded_data_for_add back to DataFrame for adding
        df_re_embedded = pd.DataFrame(re_embedded_data_for_add)
        table.add(df_re_embedded)
        
        logger.info(f"Rebuild process for table '{table_name}' completed for {len(re_embedded_data_for_add)} entries.")


# Initialize LanceDB service globally
lancedb_service = LanceDBService()
