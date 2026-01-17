import lancedb
import pandas as pd
from typing import List, Dict, Any
from loguru import logger
import numpy as np
import pyarrow as pa

from ..config import LANCEDB_PATH, EMBEDDING_DIMENSIONS

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
        data = [
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "entity_id": None, # No entity_id for family type
                "type": "family",
                "visibility": "public",
                "name": "Dòng họ Nguyễn Văn (Demo)",
                "summary": "Dòng họ có truyền thống lâu đời tại Việt Nam, nổi tiếng với nhiều nhân vật lịch sử và đóng góp cho cộng đồng."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "entity_id": "M001",
                "type": "member",
                "visibility": "public",
                "name": "Nguyễn Văn A",
                "summary": "Ông tổ đời thứ 3 của dòng họ Nguyễn, có công khai phá vùng đất mới."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "entity_id": "M002",
                "type": "member",
                "visibility": "private",
                "name": "Trần Thị B",
                "summary": "Vợ của Nguyễn Văn A, người phụ nữ hiền thục, đảm đang."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "entity_id": "M003",
                "type": "member",
                "visibility": "public",
                "name": "Lê Văn C",
                "summary": "Người con trai cả của Nguyễn Văn A, nổi tiếng với tài trí hơn người."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "entity_id": "M004",
                "type": "member",
                "visibility": "deleted", # Should not be searchable
                "name": "Phạm Thị D",
                "summary": "Thông tin thành viên đã bị xóa."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": "ANOTHER_FAMILY", # Different family, should not be searchable under F123
                "entity_id": "M005",
                "type": "member",
                "visibility": "public",
                "name": "Nguyễn Văn X",
                "summary": "Thành viên của một dòng họ khác."
            }
        ] # Added missing closing square bracket
        
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

        df = pd.DataFrame(data)
        self.db.create_table(table_name, data=df, schema=schema)
        logger.info(f"Dummy table '{table_name}' created with {len(data)} entries.")


# Initialize LanceDB service globally
lancedb_service = LanceDBService()
