import pytest
import lancedb
import uuid
import json
import os
from typing import List, Dict, Any
from unittest.mock import AsyncMock, patch

from app.core.lancedb import LanceDBService
from app.config import EMBEDDING_DIMENSIONS

# Setup a temporary LanceDB path for testing
TEST_LANCEDB_PATH = "/tmp/test_lancedb_faces"

@pytest.fixture(autouse=True)
def cleanup_lancedb():
    """Cleans up the LanceDB directory before and after each test."""
    if os.path.exists(TEST_LANCEDB_PATH):
        lancedb.connect(TEST_LANCEDB_PATH).drop_db()
    yield
    if os.path.exists(TEST_LANCEDB_PATH):
        lancedb.connect(TEST_LANCEDB_PATH).drop_db()

@pytest.fixture
def lancedb_service_instance():
    """Provides a LanceDBService instance connected to a test database."""
    with patch('app.config.LANCEDB_PATH', TEST_LANCEDB_PATH):
        service = LanceDBService()
        yield service
        # Cleanup is handled by cleanup_lancedb fixture

@pytest.fixture
def mock_embedding_service():
    """Mocks the embedding_service for consistent test results."""
    with patch('app.core.lancedb.embedding_service') as mock:
        mock.embed_query.return_value = [0.1] * EMBEDDING_DIMENSIONS
        yield mock

@pytest.mark.asyncio
async def test_add_face_data(lancedb_service_instance, mock_embedding_service):
    service = lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id = str(uuid.uuid4())
    
    face_data = {
        "family_id": family_id,
        "member_id": member_id,
        "face_id": face_id,
        "embedding": [0.2] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.99,
        "thumbnail_url": "http://example.com/thumb.jpg",
        "original_image_url": "http://example.com/original.jpg",
        "emotion": "happy",
        "emotion_confidence": 0.9,
        "vector_db_id": None,
        "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data])
    
    table_name = service._get_face_table_name(family_id)
    table = service.db.open_table(table_name)
    
    results = table.to_pandas().to_dict('records')
    assert len(results) == 1
    assert results[0]['face_id'] == face_id
    assert results[0]['member_id'] == member_id
    assert json.loads(results[0]['bounding_box']) == face_data['bounding_box']
    assert results[0]['confidence'] == face_data['confidence']
    assert results[0]['embedding'] == face_data['embedding']

@pytest.mark.asyncio
async def test_add_face_data_no_embedding_raises_error(lancedb_service_instance):
    service = lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id = str(uuid.uuid4())
    
    face_data = {
        "family_id": family_id,
        "member_id": member_id,
        "face_id": face_id,
        "embedding": None, # Missing embedding
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.99,
        "thumbnail_url": "http://example.com/thumb.jpg",
        "original_image_url": "http://example.com/original.jpg",
        "emotion": "happy",
        "emotion_confidence": 0.9,
        "vector_db_id": None,
        "is_vector_db_synced": True
    }
    
    with pytest.raises(ValueError, match="Embedding must be provided for face data."):
        await service.add_face_data(family_id, [face_data])

@pytest.mark.asyncio
async def test_search_faces_by_embedding(lancedb_service_instance, mock_embedding_service):
    service = lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id_1 = str(uuid.uuid4())
    member_id_2 = str(uuid.uuid4())
    face_id_1 = str(uuid.uuid4())
    face_id_2 = str(uuid.uuid4())
    face_id_3 = str(uuid.uuid4())
    
    # Add some dummy face data
    face_data_1 = {
        "family_id": family_id, "member_id": member_id_1, "face_id": face_id_1,
        "embedding": [0.1] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_2 = {
        "family_id": family_id, "member_id": member_id_1, "face_id": face_id_2,
        "embedding": [0.15] * EMBEDDING_DIMENSIONS, # Slightly different
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url2", "original_image_url": "orig_url2",
        "emotion": "happy", "emotion_confidence": 0.9, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_3 = {
        "family_id": family_id, "member_id": member_id_2, "face_id": face_id_3,
        "embedding": [0.8] * EMBEDDING_DIMENSIONS, # Very different
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url3", "original_image_url": "orig_url3",
        "emotion": "sad", "emotion_confidence": 0.7, "vector_db_id": None, "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data_1, face_data_2, face_data_3])
    
    query_embedding = [0.11] * EMBEDDING_DIMENSIONS # Close to face_data_1 and face_data_2
    results = await service.search_faces(family_id, query_embedding, top_k=2)
    
    assert len(results) == 2
    # The exact order and distance might vary slightly based on LanceDB's internal
    # indexing, but face_id_1 and face_id_2 should be the top results.
    result_face_ids = {res['face_id'] for res in results}
    assert face_id_1 in result_face_ids
    assert face_id_2 in result_face_ids
    
    # Test with member_id filter
    results_filtered = await service.search_faces(family_id, query_embedding, member_id=member_id_1, top_k=1)
    assert len(results_filtered) == 1
    assert results_filtered[0]['member_id'] == member_id_1

@pytest.mark.asyncio
async def test_delete_face_data_by_face_id(lancedb_service_instance):
    service = lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id_to_delete = str(uuid.uuid4())
    face_id_to_keep = str(uuid.uuid4())
    
    face_data_to_delete = {
        "family_id": family_id, "member_id": member_id, "face_id": face_id_to_delete,
        "embedding": [0.1] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_to_keep = {
        "family_id": family_id, "member_id": member_id, "face_id": face_id_to_keep,
        "embedding": [0.2] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url2", "original_image_url": "orig_url2",
        "emotion": "happy", "emotion_confidence": 0.9, "vector_db_id": None, "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data_to_delete, face_data_to_keep])
    
    deleted_count = await service.delete_face_data(family_id, face_id=face_id_to_delete)
    assert deleted_count == 1
    
    table_name = service._get_face_table_name(family_id)
    table = service.db.open_table(table_name)
    results = table.to_pandas().to_dict('records')
    assert len(results) == 1
    assert results[0]['face_id'] == face_id_to_keep

@pytest.mark.asyncio
async def test_delete_face_data_by_member_id(lancedb_service_instance):
    service = lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id_to_delete = str(uuid.uuid4())
    member_id_to_keep = str(uuid.uuid4())
    
    face_data_1 = {
        "family_id": family_id, "member_id": member_id_to_delete, "face_id": str(uuid.uuid4()),
        "embedding": [0.1] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_2 = {
        "family_id": family_id, "member_id": member_id_to_delete, "face_id": str(uuid.uuid4()),
        "embedding": [0.15] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url2", "original_image_url": "orig_url2",
        "emotion": "happy", "emotion_confidence": 0.9, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_3 = {
        "family_id": family_id, "member_id": member_id_to_keep, "face_id": str(uuid.uuid4()),
        "embedding": [0.8] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url3", "original_image_url": "orig_url3",
        "emotion": "sad", "emotion_confidence": 0.7, "vector_db_id": None, "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data_1, face_data_2, face_data_3])
    
    deleted_count = await service.delete_face_data(family_id, member_id=member_id_to_delete)
    assert deleted_count == 2
    
    table_name = service._get_face_table_name(family_id)
    table = service.db.open_table(table_name)
    results = table.to_pandas().to_dict('records')
    assert len(results) == 1
    assert results[0]['member_id'] == member_id_to_keep

@pytest.mark.asyncio
async def test_update_face_data(lancedb_service_instance):
    service = lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id_to_update = str(uuid.uuid4())
    
    face_data = {
        "family_id": family_id, "member_id": member_id, "face_id": face_id_to_update,
        "embedding": [0.1] * EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data])
    
    update_info = {
        "confidence": 0.95,
        "emotion": "angry",
        "vector_db_id": "new_vector_id"
    }
    
    updated_count = await service.update_face_data(family_id, face_id_to_update, update_info)
    assert updated_count == 1 # Assuming 1 for successful update
    
    table_name = service._get_face_table_name(family_id)
    table = service.db.open_table(table_name)
    results = table.to_pandas().to_dict('records')
    assert len(results) == 1
    assert results[0]['face_id'] == face_id_to_update
    assert results[0]['confidence'] == 0.95
    assert results[0]['emotion'] == "angry"
    assert results[0]['vector_db_id'] == "new_vector_id"
    assert results[0]['member_id'] == member_id # Should remain unchanged
