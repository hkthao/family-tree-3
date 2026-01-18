import pytest
from unittest.mock import patch, MagicMock
from app.core.lancedb import LanceDBService
from app.config import EMBEDDING_DIMENSIONS
from app.schemas.vectors import VectorData, UpdateVectorRequest, DeleteVectorRequest, RebuildVectorRequest
import numpy as np
import pyarrow as pa
import json
import pandas as pd

@pytest.fixture
def mock_lancedb_connect():
    """Fixture to mock lancedb.connect and its methods."""
    with patch('app.core.lancedb.lancedb.connect') as mock_connect:
        mock_db = MagicMock()
        mock_connect.return_value = mock_db
        yield mock_connect, mock_db

@pytest.fixture
def lancedb_service_instance(mock_lancedb_connect):
    """Fixture to provide a LanceDBService instance with mocked lancedb."""
    mock_connect, mock_db = mock_lancedb_connect
    # Reset the singleton instance for a clean test
    LanceDBService._instance = None
    service = LanceDBService()
    # The assertion for connect being called will now correctly pass here
    mock_connect.assert_called_once()
    return service, mock_connect, mock_db

@pytest.fixture
def mock_embedding_service():
    """Fixture to mock the embedding service."""
    with patch('app.core.lancedb.embedding_service') as mock_embed:
        mock_embed.embed_query.return_value = np.random.rand(EMBEDDING_DIMENSIONS).tolist()
        yield mock_embed

def test_lancedb_service_initialization(lancedb_service_instance):
    """Test that LanceDBService initializes and connects to LanceDB."""
    service, mock_connect, mock_db = lancedb_service_instance
    assert service is not None

def test_search_family_table_not_exists(lancedb_service_instance):
    """Test searching a non-existent family table."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "non_existent_family"
    mock_db.table_names.return_value = [] # Simulate no tables
    
    results = service.search_family_table(
        family_id=family_id,
        query_vector=np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
        allowed_visibility=["public"],
        top_k=5
    )
    assert results == []
    mock_db.table_names.assert_called_with()

def test_search_family_table_exists_no_results(lancedb_service_instance):
    """Test searching an existing table but with no matching results."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table
    
    # Mock the search chain
    mock_table.search.return_value.where.return_value.limit.return_value.to_list.return_value = []

    results = service.search_family_table(
        family_id=family_id,
        query_vector=np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
        allowed_visibility=["public"],
        top_k=5
    )
    assert results == []
    mock_db.open_table.assert_called_with(table_name)
    mock_table.search.assert_called_once()
    mock_table.search.return_value.where.assert_called_with("visibility = 'public'")
    mock_table.search.return_value.where.return_value.limit.assert_called_with(5)


def test_search_family_table_with_results(lancedb_service_instance):
    """
    Test searching an existing table with matching results.
    Updated to handle metadata deserialization.
    """
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    # Mock results from LanceDB, including serialized metadata
    lancedb_results = [
        {"entity_id": "M001", "name": "Nguyen Van A", "summary": "Desc A", "_distance": 0.9, "metadata": json.dumps({"original_id": "M001", "name": "Nguyen Van A", "content_type": "member"})},
        {"entity_id": "M002", "name": "Tran Thi B", "summary": "Desc B", "_distance": 0.8, "metadata": json.dumps({"original_id": "M002", "name": "Tran Thi B", "content_type": "member"})}
    ]
    mock_table.search.return_value.where.return_value.limit.return_value.to_list.return_value = lancedb_results

    results = service.search_family_table(
        family_id=family_id,
        query_vector=np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
        allowed_visibility=["public", "private"],
        top_k=2
    )

    expected_results = [
        {"metadata": {"original_id": "M001", "name": "Nguyen Van A", "content_type": "member"}, "summary": "Desc A", "score": 0.9},
        {"metadata": {"original_id": "M002", "name": "Tran Thi B", "content_type": "member"}, "summary": "Desc B", "score": 0.8}
    ]
    assert results == expected_results
    mock_db.open_table.assert_called_with(table_name)
    mock_table.search.assert_called_once()
    mock_table.search.return_value.where.assert_called_with("visibility = 'public' OR visibility = 'private'")
    mock_table.search.return_value.where.return_value.limit.assert_called_with(2)

def test_create_dummy_table_new(lancedb_service_instance, mock_embedding_service):
    """
    Test creating a dummy table when it doesn't exist, asserting the schema includes metadata.
    """
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_new"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = []

    service.create_dummy_table(family_id)

    mock_db.create_table.assert_called_once()
    args, kwargs = mock_db.create_table.call_args
    assert args[0] == table_name
    assert "data" in kwargs
    assert len(kwargs["data"]) > 0 # Check that data was provided
    assert "schema" in kwargs
    
    # Assert specific fields in the schema
    schema = kwargs["schema"]
    assert schema.field("vector").type == pa.list_(pa.float32(), EMBEDDING_DIMENSIONS)
    assert schema.field("family_id").type == pa.string()
    assert schema.field("entity_id").type == pa.string()
    assert schema.field("type").type == pa.string()
    assert schema.field("visibility").type == pa.string()
    assert schema.field("name").type == pa.string()
    assert schema.field("summary").type == pa.string()
    assert schema.field("metadata").type == pa.string() # New assertion for metadata

    mock_db.drop_table.assert_not_called()
    mock_embedding_service.embed_query.assert_called()

def test_create_dummy_table_recreate_existing(lancedb_service_instance, mock_embedding_service):
    """Test recreating a dummy table when it already exists."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_existing"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    service.create_dummy_table(family_id)

    mock_db.drop_table.assert_called_once_with(table_name)
    mock_db.create_table.assert_called_once()
    mock_embedding_service.embed_query.assert_called()

def test_add_vectors_success(lancedb_service_instance, mock_embedding_service):
    """Test adding vectors to a table."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_add"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name] # Table exists

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    vector_data = [
        VectorData(
            family_id=family_id,
            entity_id="M001",
            type="member",
            visibility="public",
            name="Test Member 1",
            summary="This is a test summary for member 1.",
            metadata={"original_id": "M001", "custom_field": "value1"}
        ),
        VectorData(
            family_id=family_id,
            entity_id="M002",
            type="member",
            visibility="private",
            name="Test Member 2",
            summary="This is another test summary for member 2.",
            metadata={"original_id": "M002", "custom_field": "value2", "nested": {"key": "val"}}
        )
    ]

    service.add_vectors(table_name, vector_data)

    mock_db.open_table.assert_called_once_with(table_name)
    mock_table.add.assert_called_once()
    mock_embedding_service.embed_query.call_count == len(vector_data)

    # Verify the data passed to table.add
    added_df = mock_table.add.call_args[0][0]
    assert isinstance(added_df, pd.DataFrame)
    assert len(added_df) == len(vector_data)
    
    # Check if metadata was serialized
    assert json.loads(added_df.loc[0, 'metadata']) == vector_data[0].metadata
    assert json.loads(added_df.loc[1, 'metadata']) == vector_data[1].metadata
    assert added_df.loc[0, 'entity_id'] == "M001"
    assert added_df.loc[0, 'type'] == "member"
    assert added_df.loc[0, 'summary'] == "This is a test summary for member 1."


def test_update_vectors_success(lancedb_service_instance, mock_embedding_service):
    """Test updating vectors in a table."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_update"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    update_request = UpdateVectorRequest(
        family_id=family_id,
        entity_id="M001",
        summary="Updated summary text.",
        visibility="private",
        metadata={"custom_update": "new_value"}
    )

    service.update_vectors(table_name, update_request)

    mock_db.open_table.assert_called_once_with(table_name)
    mock_table.update.assert_called_once()

    args, kwargs = mock_table.update.call_args
    assert kwargs["where"] == f"family_id = '{family_id}' AND entity_id = 'M001'"
    
    changes = kwargs["changes"]
    assert "vector" in changes # Vector should be re-generated
    assert changes["summary"] == "Updated summary text."
    assert changes["visibility"] == "private"
    assert json.loads(changes["metadata"]) == update_request.metadata

    mock_embedding_service.embed_query.assert_called_once_with(update_request.summary)

def test_delete_vectors_by_entity_id_success(lancedb_service_instance):
    """Test deleting vectors by entity ID."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_delete"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    delete_request = DeleteVectorRequest(
        family_id=family_id,
        entity_id="M001"
    )

    service.delete_vectors(table_name, delete_request)

    mock_db.open_table.assert_called_once_with(table_name)
    mock_table.delete.assert_called_once_with(where=f"family_id = '{family_id}' AND entity_id = 'M001'")

def test_delete_vectors_by_type_success(lancedb_service_instance):
    """Test deleting vectors by type within a family."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_delete_type"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    delete_request = DeleteVectorRequest(
        family_id=family_id,
        type="event"
    )

    service.delete_vectors(table_name, delete_request)

    mock_db.open_table.assert_called_once_with(table_name)
    mock_table.delete.assert_called_once_with(where=f"family_id = '{family_id}' AND type = 'event'")

def test_rebuild_vectors_success(lancedb_service_instance, mock_embedding_service):
    """Test rebuilding vectors for a family."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_rebuild"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    # Mock query().select().where().to_list() to return existing data with metadata
    mock_table.query.return_value.select.return_value.where.return_value.to_list.return_value = [
        {"entity_id": "M001", "summary": "Old summary M001", "type": "member", "visibility": "public", "name": "Member 1", "family_id": family_id, "metadata": json.dumps({"key": "val1"})},
        {"entity_id": "M002", "summary": "Old summary M002", "type": "member", "visibility": "public", "name": "Member 2", "family_id": family_id, "metadata": json.dumps({"key": "val2"})}
    ]

    rebuild_request = RebuildVectorRequest(family_id=family_id)

    service.rebuild_vectors(rebuild_request)

    mock_db.open_table.assert_called_once_with(table_name)
    
    # Assert select includes metadata
    mock_table.query.return_value.select.assert_called_once_with(["entity_id", "summary", "type", "visibility", "name", "family_id", "metadata"])
    
    # Assert delete and add were called
    mock_table.delete.assert_called_once_with(where=f"family_id = '{family_id}'")
    mock_table.add.assert_called_once()
    
    # Verify data added includes re-embedded vectors and original metadata
    added_df = mock_table.add.call_args[0][0]
    assert isinstance(added_df, pd.DataFrame)
    assert len(added_df) == 2
    assert added_df.loc[0, 'summary'] == "Old summary M001"
    assert json.loads(added_df.loc[0, 'metadata']) == {"key": "val1"}
    assert "vector" in added_df.columns
    mock_embedding_service.embed_query.call_count == 2

def test_rebuild_vectors_specific_entities_success(lancedb_service_instance, mock_embedding_service):
    """Test rebuilding vectors for specific entities."""
    service, mock_connect, mock_db = lancedb_service_instance
    family_id = "F123_rebuild_specific"
    table_name = f"family_{family_id}"
    mock_db.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_db.open_table.return_value = mock_table

    # Mock query().select().where().to_list() to return existing data with metadata
    mock_table.query.return_value.select.return_value.where.return_value.to_list.return_value = [
        {"entity_id": "M001", "summary": "Old summary M001", "type": "member", "visibility": "public", "name": "Member 1", "family_id": family_id, "metadata": json.dumps({"key": "val1"})}
    ]

    rebuild_request = RebuildVectorRequest(family_id=family_id, entity_ids=["M001"])

    service.rebuild_vectors(rebuild_request)

    mock_db.open_table.assert_called_once_with(table_name)
    
    # Assert select includes metadata
    mock_table.query.return_value.select.assert_called_once_with(["entity_id", "summary", "type", "visibility", "name", "family_id", "metadata"])
    
    # Assert delete and add were called with correct filter
    mock_table.delete.assert_called_once_with(where=f"family_id = '{family_id}' AND entity_id IN ('M001')")
    mock_table.add.assert_called_once()
    
    # Verify data added includes re-embedded vectors and original metadata
    added_df = mock_table.add.call_args[0][0]
    assert isinstance(added_df, pd.DataFrame)
    assert len(added_df) == 1
    assert added_df.loc[0, 'summary'] == "Old summary M001"
    assert json.loads(added_df.loc[0, 'metadata']) == {"key": "val1"}
