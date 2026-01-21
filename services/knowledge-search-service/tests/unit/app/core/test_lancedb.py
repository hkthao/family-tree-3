# tests/unit/app/core/test_lancedb.py
import pytest
from unittest.mock import MagicMock, AsyncMock, patch
import numpy as np
import pandas as pd
import pyarrow as pa
import json
from uuid import UUID

from app.core.lancedb import (
    LanceDBBaseService,
    KnowledgeLanceDBService,
    FaceLanceDBService,
)
from app.core.embeddings import EmbeddingService
from app.schemas.vectors import (
    VectorData,
    UpdateVectorRequest,
    DeleteVectorRequest,
    RebuildVectorRequest,
)
from app.schemas.lancedb_schemas import KNOWLEDGE_LANCEDB_SCHEMA, FACE_LANCEDB_SCHEMA
from app.config import LANCEDB_PATH


# Fixture for mocking LanceDB connection and table operations
@pytest.fixture
def mock_lancedb_connection(mocker):
    mock_db = MagicMock()
    mock_db.table_names.return_value = []  # Initially no tables
    mocker.patch("lancedb.connect", return_value=mock_db)
    return mock_db


# Fixture for mocking EmbeddingService
@pytest.fixture
def mock_embedding_service(mocker):
    mock_service = MagicMock(spec=EmbeddingService)
    mock_service.embed_query.return_value = np.array([0.1, 0.2, 0.3]).tolist()
    return mock_service


# Fixture to reset LanceDBBaseService _instance for test isolation
@pytest.fixture(autouse=True)
def reset_lancedb_base_service_instance(mocker):
    # This ensures that each test gets a fresh instance if LanceDBBaseService
    # were to be a singleton, though it's not explicitly defined as such.
    # It's good practice to ensure no state carries over between tests for services.
    pass


@pytest.mark.asyncio
class TestLanceDBBaseService:
    async def test_create_table_if_not_exists_new_table(self, mock_lancedb_connection):
        service = LanceDBBaseService()
        table_name = "test_table"
        expected_schema = KNOWLEDGE_LANCEDB_SCHEMA

        mock_table = MagicMock()
        mock_table.schema = expected_schema  # Mock schema property
        mock_lancedb_connection.create_table.return_value = mock_table
        mock_lancedb_connection.table_names.return_value = []

        await service._create_table_if_not_exists(table_name, expected_schema)

        mock_lancedb_connection.create_table.assert_called_once_with(
            table_name, schema=expected_schema
        )
        mock_lancedb_connection.open_table.assert_not_called()

    async def test_create_table_if_not_exists_table_exists_compatible_schema(
        self, mock_lancedb_connection
    ):
        service = LanceDBBaseService()
        table_name = "test_table"
        expected_schema = KNOWLEDGE_LANCEDB_SCHEMA

        mock_table = MagicMock()
        mock_table.schema = expected_schema
        mock_lancedb_connection.table_names.return_value = [table_name]
        mock_lancedb_connection.open_table.return_value = mock_table

        await service._create_table_if_not_exists(table_name, expected_schema)

        mock_lancedb_connection.open_table.assert_called_once_with(table_name)
        mock_lancedb_connection.create_table.assert_not_called()
        mock_lancedb_connection.drop_table.assert_not_called()

    async def test_create_table_if_not_exists_table_exists_incompatible_schema(
        self, mock_lancedb_connection
    ):
        service = LanceDBBaseService()
        table_name = "test_table"

        # Create a schema with different vector dimension
        incompatible_schema_fields = list(KNOWLEDGE_LANCEDB_SCHEMA)
        # Modify the 'vector' field to have a different list_size
        for i, field in enumerate(incompatible_schema_fields):
            if field.name == "vector":
                incompatible_schema_fields[i] = pa.field(
                    "vector", pa.list_(pa.float32(), 129)
                )  # Original is 128
                break
        incompatible_schema = pa.schema(incompatible_schema_fields)

        mock_existing_table = MagicMock()
        mock_existing_table.schema = incompatible_schema
        mock_lancedb_connection.table_names.return_value = [table_name]
        mock_lancedb_connection.open_table.return_value = mock_existing_table

        mock_recreated_table = MagicMock()
        mock_recreated_table.schema = KNOWLEDGE_LANCEDB_SCHEMA
        mock_lancedb_connection.create_table.return_value = mock_recreated_table

        await service._create_table_if_not_exists(
            table_name, KNOWLEDGE_LANCEDB_SCHEMA
        )

        mock_lancedb_connection.open_table.assert_called_once_with(table_name)
        mock_lancedb_connection.drop_table.assert_called_once_with(table_name)
        mock_lancedb_connection.create_table.assert_called_once_with(
            table_name, schema=KNOWLEDGE_LANCEDB_SCHEMA
        )


@pytest.mark.asyncio
class TestKnowledgeLanceDBService:
    @pytest.fixture(autouse=True)
    def setup_knowledge_service(
        self, mock_lancedb_connection, mock_embedding_service
    ):
        # We need to ensure that the LanceDBBaseService.__init__ is called
        # but also that `lancedb.connect` is mocked.
        # Here, we ensure LanceDBBaseService uses the mocked connection.
        with patch("lancedb.connect", return_value=mock_lancedb_connection):
            self.service = KnowledgeLanceDBService(mock_embedding_service)
        self.mock_db = mock_lancedb_connection
        self.mock_embedding_service = mock_embedding_service
        self.family_id = "F123"
        self.table_name = f"family_knowledge_{self.family_id}"

        # Mock the open_table method which is called frequently
        self.mock_table_instance = MagicMock()
        self.mock_table_instance.delete = AsyncMock()
        self.mock_db.open_table.return_value = self.mock_table_instance
        self.mock_db.table_names.return_value = [self.table_name] # Assume table exists for most tests


    async def test_add_vectors(self):
        vector_data = [
            VectorData(
                family_id=self.family_id,
                entity_id="E001",
                type="person",
                visibility="public",
                name="Test Person",
                summary="A test person for knowledge base.",
                metadata={"age": 30, "city": "Anytown"},
            )
        ]
        expected_vector = [0.1, 0.2, 0.3]

        await self.service.add_vectors(self.table_name, vector_data)

        self.mock_embedding_service.embed_query.assert_called_once_with(
            vector_data[0].summary
        )
        self.mock_table_instance.add.assert_called_once()
        added_df = self.mock_table_instance.add.call_args[0][0]
        assert isinstance(added_df, pd.DataFrame)
        assert len(added_df) == 1
        assert added_df["entity_id"].iloc[0] == "E001"
        assert added_df["vector"].iloc[0] == expected_vector
        assert json.loads(added_df["metadata"].iloc[0]) == vector_data[0].metadata

    async def test_add_vectors_empty_data(self):
        await self.service.add_vectors(self.table_name, [])
        self.mock_embedding_service.embed_query.assert_not_called()
        self.mock_table_instance.add.assert_not_called()

    async def test_update_vectors_summary_change(self):
        update_request = UpdateVectorRequest(
            family_id=self.family_id,
            entity_id="E001",
            summary="Updated summary for the test person.",
            metadata={"age": 31, "status": "active"},
        )
        expected_new_vector = [0.1, 0.2, 0.3] # from mock_embedding_service

        self.service.update_vectors(self.table_name, update_request)

        self.mock_embedding_service.embed_query.assert_called_once_with(
            update_request.summary
        )
        self.mock_table_instance.update.assert_called_once()
        call_args = self.mock_table_instance.update.call_args[1]
        assert call_args["where"] == f"family_id = '{self.family_id}' AND entity_id = 'E001'"
        assert call_args["changes"]["summary"] == update_request.summary
        assert call_args["changes"]["vector"] == expected_new_vector
        assert json.loads(call_args["changes"]["metadata"]) == update_request.metadata

    async def test_update_vectors_no_summary_change(self):
        update_request = UpdateVectorRequest(
            family_id=self.family_id,
            entity_id="E001",
            metadata={"status": "inactive"},
        )

        self.service.update_vectors(self.table_name, update_request)

        self.mock_embedding_service.embed_query.assert_not_called()
        self.mock_table_instance.update.assert_called_once()
        call_args = self.mock_table_instance.update.call_args[1]
        assert call_args["where"] == f"family_id = '{self.family_id}' AND entity_id = 'E001'"
        assert "summary" not in call_args["changes"]
        assert "vector" not in call_args["changes"]
        assert json.loads(call_args["changes"]["metadata"]) == update_request.metadata

    async def test_delete_vectors_by_entity_id(self):
        delete_request = DeleteVectorRequest(
            family_id=self.family_id, entity_id="E001"
        )
        self.mock_table_instance.delete.return_value = 1

        deleted_count = await self.service.delete_vectors(
            self.table_name, delete_request
        )

        self.mock_table_instance.delete.assert_called_once_with(
            where=f"family_id = '{self.family_id}' AND entity_id = 'E001'"
        )
        assert deleted_count == 1

    async def test_delete_vectors_by_type(self):
        delete_request = DeleteVectorRequest(
            family_id=self.family_id, type="person"
        )
        self.mock_table_instance.delete.return_value = 2

        deleted_count = await self.service.delete_vectors(
            self.table_name, delete_request
        )

        self.mock_table_instance.delete.assert_called_once_with(
            where=f"family_id = '{self.family_id}' AND type = 'person'"
        )
        assert deleted_count == 2

    async def test_delete_vectors_by_family_id_only(self):
        delete_request = DeleteVectorRequest(family_id=self.family_id)
        self.mock_table_instance.delete.return_value = 3

        deleted_count = await self.service.delete_vectors(
            self.table_name, delete_request
        )

        self.mock_table_instance.delete.assert_called_once_with(
            where=f"family_id = '{self.family_id}'"
        )
        assert deleted_count == 3

    async def test_rebuild_vectors(self):
        rebuild_request = RebuildVectorRequest(
            family_id=self.family_id, entity_ids=["E001", "E002"]
        )

        # Mock query().select().where().to_list() chain
        mock_query_result = [
            {"entity_id": "E001", "summary": "Summary 1", "type": "person",
             "visibility": "public", "name": "Name 1",
             "family_id": self.family_id,
             "metadata": json.dumps({"key": "value1"})},
            {"entity_id": "E002", "summary": "Summary 2", "type": "person",
             "visibility": "public", "name": "Name 2",
             "family_id": self.family_id,
             "metadata": json.dumps({"key": "value2"})},
        ]
        self.mock_table_instance.query.return_value.select.return_value.where.return_value.to_list.return_value = (
            mock_query_result
        )
        self.mock_embedding_service.embed_query.side_effect = (
            lambda x: [0.1, 0.2, 0.3] if x == "Summary 1" else [0.4, 0.5, 0.6]
        )
        self.mock_table_instance.delete.return_value = 2
        
        # Ensure _create_table_if_not_exists is mocked to prevent actual table creation
        with patch.object(self.service, '_create_table_if_not_exists', new=AsyncMock()) as mock_create_table:
            await self.service.rebuild_vectors(rebuild_request)

            mock_create_table.assert_not_called() # Should not be called if table exists
            self.mock_table_instance.delete.assert_called_once_with(
                where=f"family_id = '{self.family_id}' AND entity_id IN ('E001', 'E002')"
            )
            assert self.mock_embedding_service.embed_query.call_count == 2
            self.mock_table_instance.add.assert_called_once()
            added_df = self.mock_table_instance.add.call_args[0][0]
            assert len(added_df) == 2
            assert added_df["entity_id"].iloc[0] == "E001"
            assert added_df["vector"].iloc[0] == [0.1, 0.2, 0.3]
            assert added_df["entity_id"].iloc[1] == "E002"
            assert added_df["vector"].iloc[1] == [0.4, 0.5, 0.6]

    async def test_search_knowledge_table_success(self):
        query_vector = [0.1, 0.2, 0.3]
        allowed_visibility = ["public"]
        top_k = 1

        mock_search_result = [
            {"metadata": json.dumps({"age": 30}), "summary": "Test Summary",
             "_distance": 0.05}
        ]
        self.mock_table_instance.search.return_value.where.return_value.limit.return_value.to_list.return_value = (
            mock_search_result
        )

        results = self.service.search_knowledge_table(
            self.family_id, query_vector, allowed_visibility, top_k
        )

        self.mock_table_instance.search.assert_called_once_with(query_vector)
        self.mock_table_instance.search.return_value.where.assert_called_once_with(
            "visibility = 'public'"
        )
        self.mock_table_instance.search.return_value.where.return_value.limit.assert_called_once_with(
            top_k
        )
        assert len(results) == 1
        assert results[0]["summary"] == "Test Summary"
        assert results[0]["score"] == 0.05
        assert results[0]["metadata"] == {"age": 30}

    async def test_search_knowledge_table_no_table(self, mock_lancedb_connection):
        mock_lancedb_connection.table_names.return_value = [] # Ensure no table exists
        query_vector = [0.1, 0.2, 0.3]
        allowed_visibility = ["public"]
        top_k = 1

        service = KnowledgeLanceDBService(self.mock_embedding_service)
        results = service.search_knowledge_table(
            self.family_id, query_vector, allowed_visibility, top_k
        )
        assert results == []
        mock_lancedb_connection.open_table.assert_not_called()

    async def test_search_knowledge_table_exception(self):
        query_vector = [0.1, 0.2, 0.3]
        allowed_visibility = ["public"]
        top_k = 1

        self.mock_table_instance.search.side_effect = Exception("Search error")

        results = self.service.search_knowledge_table(
            self.family_id, query_vector, allowed_visibility, top_k
        )

        assert results == []


@pytest.mark.asyncio
class TestFaceLanceDBService:
    @pytest.fixture(autouse=True)
    def setup_face_service(self, mock_lancedb_connection):
        with patch("lancedb.connect", return_value=mock_lancedb_connection):
            self.service = FaceLanceDBService()
        self.mock_db = mock_lancedb_connection
        self.family_id = str(UUID("f47ac10b-58cc-4372-a567-0e02b2c3d479"))
        self.table_name = f"faces_{self.family_id}"

        self.mock_table_instance = MagicMock()
        self.mock_db.open_table.return_value = self.mock_table_instance
        self.mock_db.table_names.return_value = [self.table_name] # Assume table exists for most tests

    async def test_add_face_data(self):
        face_data = [
            {
                "family_id": UUID("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                "face_id": UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479"),
                "member_id": UUID("e47ac10b-58cc-4372-a567-0e02b2c3d479"),
                "bounding_box": {"x": 10, "y": 20, "width": 30, "height": 40},
                "confidence": 0.95,
                "thumbnail_url": "http://example.com/thumb.jpg",
                "original_image_url": "http://example.com/original.jpg",
                "emotion": "happy",
                "emotion_confidence": 0.99,
                "vector_db_id": "vdb1",
                "is_vector_db_synced": True,
                "embedding": [0.1, 0.1, 0.1],
            }
        ]

        await self.service.add_face_data(self.family_id, face_data)

        self.mock_table_instance.add.assert_called_once()
        added_df = self.mock_table_instance.add.call_args[0][0]
        assert isinstance(added_df, pd.DataFrame)
        assert len(added_df) == 1
        assert added_df["face_id"].iloc[0] == str(face_data[0]["face_id"])
        assert added_df["vector"].iloc[0] == face_data[0]["embedding"]
        assert json.loads(added_df["bounding_box"].iloc[0]) == face_data[0]["bounding_box"]

    async def test_add_face_data_no_embedding_raises_error(self):
        face_data = [
            {
                "family_id": UUID("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                "face_id": UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479"),
                "member_id": UUID("e47ac10b-58cc-4372-a567-0e02b2c3d479"),
                "bounding_box": {"x": 10, "y": 20, "width": 30, "height": 40},
                "confidence": 0.95,
                "thumbnail_url": "http://example.com/thumb.jpg",
                "original_image_url": "http://example.com/original.jpg",
                "emotion": "happy",
                "emotion_confidence": 0.99,
                "vector_db_id": "vdb1",
                "is_vector_db_synced": True,
                # "embedding": [0.1, 0.1, 0.1], # Missing embedding
            }
        ]

        with pytest.raises(ValueError, match="Embedding must be provided for face data."):
            await self.service.add_face_data(self.family_id, face_data)
        self.mock_table_instance.add.assert_not_called()

    async def test_search_faces_success(self):
        query_embedding = [0.1, 0.1, 0.1]
        member_id = str(UUID("e47ac10b-58cc-4372-a567-0e02b2c3d479"))
        top_k = 1

        mock_search_result = [
            {
                "family_id": str(UUID("f47ac10b-58cc-4372-a567-0e02b2c3d479")),
                "face_id": str(UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479")),
                "member_id": member_id,
                "bounding_box": json.dumps({"x": 10, "y": 20, "width": 30, "height": 40}),
                "confidence": 0.95,
                "thumbnail_url": "http://example.com/thumb.jpg",
                "original_image_url": "http://example.com/original.jpg",
                "emotion": "happy",
                "emotion_confidence": 0.99,
                "vector_db_id": "vdb1",
                "is_vector_db_synced": True,
                "_distance": 0.01,
            }
        ]
        self.mock_table_instance.search.return_value.where.return_value.select.return_value.limit.return_value.to_list.return_value = (
            mock_search_result
        )

        results = await self.service.search_faces(
            self.family_id, query_embedding, member_id, top_k
        )

        self.mock_table_instance.search.assert_called_once_with(query_embedding)
        self.mock_table_instance.search.return_value.where.assert_called_once_with(
            f"member_id = '{member_id}'"
        )
        assert len(results) == 1
        assert results[0]["face_id"] == UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479")
        assert results[0]["score"] == 0.01
        assert results[0]["bounding_box"] == {"x": 10, "y": 20, "width": 30, "height": 40}

    async def test_search_faces_no_table(self, mock_lancedb_connection):
        mock_lancedb_connection.table_names.return_value = [] # Ensure no table exists
        query_embedding = [0.1, 0.1, 0.1]
        
        service = FaceLanceDBService()
        results = await service.search_faces(self.family_id, query_embedding)
        assert results == []
        mock_lancedb_connection.open_table.assert_not_called()

    async def test_search_faces_no_query_embedding_raises_error(self):
        with pytest.raises(ValueError, match="Query embedding must be provided for face search."):
            await self.service.search_faces(self.family_id, [])

    async def test_delete_face_data_by_face_id(self):
        face_id = str(UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479"))
        self.mock_table_instance.delete.return_value = 1

        deleted_count = await self.service.delete_face_data(
            self.family_id, face_id=face_id
        )

        self.mock_table_instance.delete.assert_called_once_with(
            where=f"family_id = '{self.family_id}' AND face_id = '{face_id}'"
        )
        assert deleted_count == 1

    async def test_delete_face_data_by_member_id(self):
        member_id = str(UUID("e47ac10b-58cc-4372-a567-0e02b2c3d479"))
        self.mock_table_instance.delete.return_value = 2

        deleted_count = await self.service.delete_face_data(
            self.family_id, member_id=member_id
        )

        self.mock_table_instance.delete.assert_called_once_with(
            where=f"family_id = '{self.family_id}' AND member_id = '{member_id}'"
        )
        assert deleted_count == 2

    async def test_delete_face_data_by_family_id_only(self):
        self.mock_table_instance.delete.return_value = 3

        deleted_count = await self.service.delete_face_data(self.family_id)

        self.mock_table_instance.delete.assert_called_once_with(
            where=f"family_id = '{self.family_id}'"
        )
        assert deleted_count == 3

    async def test_delete_faces_by_family_id(self):
        await self.service.delete_faces_by_family_id(self.family_id)

        self.mock_db.drop_table.assert_called_once_with(self.table_name)

    async def test_delete_faces_by_family_id_no_table(self, mock_lancedb_connection):
        mock_lancedb_connection.table_names.return_value = [] # No table to drop
        service = FaceLanceDBService()
        await service.delete_faces_by_family_id(self.family_id)
        mock_lancedb_connection.drop_table.assert_not_called()

    async def test_update_face_data_embedding_change(self):
        face_id = str(UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479"))
        update_info = {
            "embedding": [0.2, 0.2, 0.2],
            "confidence": 0.98,
            "bounding_box": {"x": 50, "y": 60, "width": 70, "height": 80},
        }
        self.mock_table_instance.update.return_value = 1

        updated_count = await self.service.update_face_data(
            self.family_id, face_id, update_info
        )

        self.mock_table_instance.update.assert_called_once()
        call_args = self.mock_table_instance.update.call_args[1]
        assert call_args["where"] == f"family_id = '{self.family_id}' AND face_id = '{face_id}'"
        assert call_args["changes"]["vector"] == update_info["embedding"]
        assert call_args["changes"]["confidence"] == update_info["confidence"]
        assert json.loads(call_args["changes"]["bounding_box"]) == update_info["bounding_box"]
        assert "embedding" not in call_args["changes"] # Should be replaced by 'vector'
        assert updated_count == 1

    async def test_update_face_data_no_changes(self):
        face_id = str(UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479"))
        update_info = {} # No updates

        self.mock_table_instance.update.return_value = 0
        updated_count = await self.service.update_face_data(
            self.family_id, face_id, update_info
        )

        self.mock_table_instance.update.assert_not_called()
        assert updated_count == 0

    async def test_update_face_data_no_table(self, mock_lancedb_connection):
        mock_lancedb_connection.table_names.return_value = [] # No table exists
        face_id = str(UUID("d47ac10b-58cc-4372-a567-0e02b2c3d479"))
        update_info = {"confidence": 0.9}

        service = FaceLanceDBService()
        updated_count = await service.update_face_data(
            self.family_id, face_id, update_info
        )

        assert updated_count == 0
        self.mock_table_instance.update.assert_not_called()
