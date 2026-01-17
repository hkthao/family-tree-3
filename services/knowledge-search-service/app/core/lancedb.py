import lancedb
import pandas as pd
from typing import List, Dict, Any
from loguru import logger
import numpy as np

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
                    "member_id": res.get("member_id"),
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
                "member_id": "M001",
                "visibility": "public",
                "name": "Nguyễn Văn A",
                "summary": "Ông tổ đời thứ 3 của dòng họ Nguyễn, có công khai phá vùng đất mới."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "member_id": "M002",
                "visibility": "private",
                "name": "Trần Thị B",
                "summary": "Vợ của Nguyễn Văn A, người phụ nữ hiền thục, đảm đang."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "member_id": "M003",
                "visibility": "public",
                "name": "Lê Văn C",
                "summary": "Người con trai cả của Nguyễn Văn A, nổi tiếng với tài trí hơn người."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": family_id,
                "member_id": "M004",
                "visibility": "deleted", # Should not be searchable
                "name": "Phạm Thị D",
                "summary": "Thông tin thành viên đã bị xóa."
            },
            {
                "vector": np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
                "family_id": "ANOTHER_FAMILY", # Different family, should not be searchable under F123
                "member_id": "M005",
                "visibility": "public",
                "name": "Nguyễn Văn X",
                "summary": "Thành viên của một dòng họ khác."
            }
        ]
        
        # Ensure vector column is of float[EMBEDDING_DIMENSIONS] type
        schema = {
            "vector": f"list<float:{EMBEDDING_DIMENSIONS}>",
            "family_id": "string",
            "member_id": "string",
            "visibility": "string",
            "name": "string",
            "summary": "string"
        }

        df = pd.DataFrame(data)
        self.db.create_table(table_name, data=df, schema=schema)
        logger.info(f"Dummy table '{table_name}' created with {len(data)} entries.")


# Initialize LanceDB service globally
lancedb_service = LanceDBService()
