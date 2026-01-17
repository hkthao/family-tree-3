import pytest
from unittest.mock import patch, MagicMock
from services.knowledge_search_service.app.core.lancedb import LanceDBService
from services.knowledge_search_service.app.config import EMBEDDING_DIMENSIONS
import numpy as np

@pytest.fixture
def mock_lancedb_connection():
    """Fixture to mock lancedb.connect and its methods."""
    with patch('services.knowledge_search_service.app.core.lancedb.lancedb') as mock_lancedb:
        mock_db = MagicMock()
        mock_lancedb.connect.return_value = mock_db
        yield mock_db

@pytest.fixture
def lancedb_service_instance(mock_lancedb_connection):
    """Fixture to provide a LanceDBService instance with mocked lancedb."""
    # Reset the singleton instance for a clean test
    LanceDBService._instance = None
    service = LanceDBService()
    # Ensure the connect method was called
    mock_lancedb_connection.connect.assert_called_once()
    return service

def test_lancedb_service_initialization(mock_lancedb_connection, lancedb_service_instance):
    """Test that LanceDBService initializes and connects to LanceDB."""
    assert lancedb_service_instance is not None

def test_search_family_table_not_exists(mock_lancedb_connection, lancedb_service_instance):
    """Test searching a non-existent family table."""
    family_id = "non_existent_family"
    mock_lancedb_connection.table_names.return_value = [] # Simulate no tables
    
    results = lancedb_service_instance.search_family_table(
        family_id=family_id,
        query_vector=np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
        allowed_visibility=["public"],
        top_k=5
    )
    assert results == []
    mock_lancedb_connection.table_names.assert_called_with()

def test_search_family_table_exists_no_results(mock_lancedb_connection, lancedb_service_instance):
    """Test searching an existing table but with no matching results."""
    family_id = "F123"
    table_name = f"family_{family_id}"
    mock_lancedb_connection.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_lancedb_connection.open_table.return_value = mock_table
    
    # Mock the search chain
    mock_table.search.return_value.where.return_value.limit.return_value.to_list.return_value = []

    results = lancedb_service_instance.search_family_table(
        family_id=family_id,
        query_vector=np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
        allowed_visibility=["public"],
        top_k=5
    )
    assert results == []
    mock_lancedb_connection.open_table.assert_called_with(table_name)
    mock_table.search.assert_called_once()
    mock_table.search.return_value.where.assert_called_with("visibility = 'public'")
    mock_table.search.return_value.where.return_value.limit.assert_called_with(5)


def test_search_family_table_with_results(mock_lancedb_connection, lancedb_service_instance):
    """Test searching an existing table with matching results."""
    family_id = "F123"
    table_name = f"family_{family_id}"
    mock_lancedb_connection.table_names.return_value = [table_name]

    mock_table = MagicMock()
    mock_lancedb_connection.open_table.return_value = mock_table

    # Mock results from LanceDB
    lancedb_results = [
        {"member_id": "M001", "name": "Nguyen Van A", "summary": "Desc A", "_distance": 0.9},
        {"member_id": "M002", "name": "Tran Thi B", "summary": "Desc B", "_distance": 0.8}
    ]
    mock_table.search.return_value.where.return_value.limit.return_value.to_list.return_value = lancedb_results

    results = lancedb_service_instance.search_family_table(
        family_id=family_id,
        query_vector=np.random.rand(EMBEDDING_DIMENSIONS).tolist(),
        allowed_visibility=["public", "private"],
        top_k=2
    )

    expected_results = [
        {"member_id": "M001", "name": "Nguyen Van A", "summary": "Desc A", "score": 0.9},
        {"member_id": "M002", "name": "Tran Thi B", "summary": "Desc B", "score": 0.8}
    ]
    assert results == expected_results
    mock_lancedb_connection.open_table.assert_called_with(table_name)
    mock_table.search.assert_called_once()
    mock_table.search.return_value.where.assert_called_with("visibility = 'public' OR visibility = 'private'")
    mock_table.search.return_value.where.return_value.limit.assert_called_with(2)

def test_create_dummy_table_new(mock_lancedb_connection, lancedb_service_instance):
    """Test creating a dummy table when it doesn't exist."""
    family_id = "F123_new"
    table_name = f"family_{family_id}"
    mock_lancedb_connection.table_names.return_value = []

    lancedb_service_instance.create_dummy_table(family_id)

    mock_lancedb_connection.create_table.assert_called_once()
    args, kwargs = mock_lancedb_connection.create_table.call_args
    assert args[0] == table_name
    assert "data" in kwargs
    assert len(kwargs["data"]) > 0 # Check that data was provided
    assert "schema" in kwargs
    assert kwargs["schema"]["vector"] == f"list<float:{EMBEDDING_DIMENSIONS}>"
    mock_lancedb_connection.drop_table.assert_not_called()

def test_create_dummy_table_recreate_existing(mock_lancedb_connection, lancedb_service_instance):
    """Test recreating a dummy table when it already exists."""
    family_id = "F123_existing"
    table_name = f"family_{family_id}"
    mock_lancedb_connection.table_names.return_value = [table_name]

    lancedb_service_instance.create_dummy_table(family_id)

    mock_lancedb_connection.drop_table.assert_called_once_with(table_name)
    mock_lancedb_connection.create_table.assert_called_once()
