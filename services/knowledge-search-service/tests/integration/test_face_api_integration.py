import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch # Import patch
import json
import os
import lancedb
import uuid # Import uuid
import shutil # Import shutil
import numpy as np # Import numpy

# Import necessary components for adding dummy data
from app.core.lancedb import face_lancedb_service
from app.models.face_models import FaceMetadata
from app.config import FACE_EMBEDDING_DIMENSIONS

# Setup a temporary LanceDB path for integration testing
TEST_LANCEDB_PATH_INTEGRATION = "/tmp/test_lancedb_integration"

@pytest.fixture(scope="module", autouse=True)
def client_with_patched_lancedb():
    """Provides a TestClient with LANCEDB_PATH patched for the duration of the module."""
    # Ensure the directory exists but is empty before tests
    if os.path.exists(TEST_LANCEDB_PATH_INTEGRATION):
        shutil.rmtree(TEST_LANCEDB_PATH_INTEGRATION)
    
    with patch('app.config.LANCEDB_PATH', TEST_LANCEDB_PATH_INTEGRATION):
        # Import app AFTER patching is in place
        from app.main import app
        with TestClient(app) as client:
            yield client
    
    # Cleanup after tests
    if os.path.exists(TEST_LANCEDB_PATH_INTEGRATION):
        shutil.rmtree(TEST_LANCEDB_PATH_INTEGRATION)

@pytest.fixture(scope="module")
async def setup_dummy_face_data():
    """Adds dummy face data to LanceDB for testing search."""
    # This fixture uses the patched LANCEDB_PATH from client_with_patched_lancedb fixture
    # but since it's an async fixture, we need to ensure the service instance
    # is using the correct path when add_face_data is called.
    # The global face_lancedb_service instance is initialized when app.main is imported.
    # To ensure it uses the patched path, we need to re-initialize it or ensure it's
    # correctly mocked/patched in this context.
    # For integration tests, it's better to let the actual service initialize with the patched path.

@pytest.fixture(scope="module")
async def setup_dummy_face_data():
    """Adds dummy face data to LanceDB for testing search."""
    # Define a consistent family_id for testing within this fixture
    test_family_id = uuid.UUID("57bbd62b-2731-4f21-a5f1-d33e7b9b5902")
    test_member_id = uuid.uuid4()
    test_face_id = uuid.uuid4()
    
    # Create a dummy face data entry with a known embedding
    # The embedding should match FACE_EMBEDDING_DIMENSIONS (512)
    dummy_embedding = (np.random.rand(FACE_EMBEDDING_DIMENSIONS) * 2 - 1).tolist() # Random embedding between -1 and 1

    dummy_face_metadata = FaceMetadata(
        family_id=test_family_id,
        member_id=test_member_id,
        face_id=test_face_id,
        embedding=dummy_embedding,
        bounding_box={"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        confidence=0.9,
        thumbnail_url="http://example.com/dummy_thumb.jpg",
        original_image_url="http://example.com/dummy_original.jpg",
        emotion="neutral",
        emotion_confidence=0.8,
        vector_db_id="dummy_vdb_id",
        is_vector_db_synced=True
    )
    
    dummy_face_data_dict = dummy_face_metadata.model_dump()
    dummy_face_data_dict["family_id"] = str(dummy_face_data_dict["family_id"])
    dummy_face_data_dict["member_id"] = str(dummy_face_data_dict["member_id"])
    dummy_face_data_dict["face_id"] = str(dummy_face_data_dict["face_id"])

    # Ensure the table is dropped before adding data to guarantee fresh schema
    table_name = face_lancedb_service._get_face_table_name(str(test_family_id))
    if table_name in face_lancedb_service.db.table_names():
        face_lancedb_service.db.drop_table(table_name)
    
    await face_lancedb_service.add_face_data(str(test_family_id), [dummy_face_data_dict])
    return test_face_id, dummy_embedding, test_family_id # Return test_family_id as well # Change yield to return
    # No explicit cleanup for this data needed, as client_with_patched_lancedb will rmtree the db

@pytest.mark.asyncio
async def test_search_faces_integration(client_with_patched_lancedb, setup_dummy_face_data):
    """
    Test case for POST /api/v1/faces/search endpoint using a generated query_embedding.
    """
    dummy_face_id, dummy_embedding, test_family_id = await setup_dummy_face_data
    
    # Create request payload directly in code
    request_payload = {
        "query_embedding": dummy_embedding, # Use the embedding from the dummy data
        "family_id": str(test_family_id),
        "top_k": 1
    }

    headers = {"Content-Type": "application/json"}

    response = client_with_patched_lancedb.post("/api/v1/faces/search", headers=headers, json=request_payload)

    assert response.status_code == 200
    response_data = response.json()

    assert "results" in response_data
    assert isinstance(response_data["results"], list)
    assert len(response_data["results"]) >= 1 # Expect at least the dummy data

    # Check for structure and the dummy data
    found_dummy_face = False
    for result_item in response_data["results"]:
        assert "face_id" in result_item
        assert "member_id" in result_item
        assert "score" in result_item
        assert "metadata" in result_item
        assert isinstance(result_item["score"], float)
        assert isinstance(result_item["face_id"], str) # UUID will be returned as string
        assert isinstance(result_item["member_id"], str) # UUID will be returned as string
        assert isinstance(result_item["metadata"], dict)
        if result_item["face_id"] == str(dummy_face_id):
            found_dummy_face = True
    
    assert found_dummy_face, "Dummy face data was not found in search results."

