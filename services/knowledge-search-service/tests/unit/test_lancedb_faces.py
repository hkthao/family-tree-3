import pytest
import lancedb
import uuid
import json
import numpy as np
import os
from unittest.mock import patch, MagicMock

from app.core.lancedb import FaceLanceDBService # Changed from LanceDBService
from app.config import FACE_EMBEDDING_DIMENSIONS
from app.models.face_models import FaceMetadata # Import FaceMetadata, it's used in tests

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
def face_lancedb_service_instance(): # Renamed fixture
    """Provides a FaceLanceDBService instance connected to a test database."""
    with patch('app.config.LANCEDB_PATH', TEST_LANCEDB_PATH), \
         patch('lancedb.connect') as mock_lancedb_connect:
        
        mock_db = MagicMock()
        mock_lancedb_connect.return_value = mock_db
        
        # This will hold the "data" for our mocked LanceDB table
        mock_table_data = [] 
        _mock_created_tables = [] # List to track names of "created" tables

        mock_table = MagicMock()
        # Ensure open_table always returns our mock_table for any table name
        mock_db.open_table.return_value = mock_table

        # Mock add method
        def mock_add(df):
            # Convert UUIDs back to string for comparison with mock_table_data
            df_dict = df.to_dict('records')
            for row in df_dict:
                for k, v in row.items():
                    if isinstance(v, uuid.UUID):
                        row[k] = str(v)
                # Ensure bounding_box is a JSON string, as it's stored that way in LanceDB
                if 'bounding_box' in row and isinstance(row['bounding_box'], dict):
                    row['bounding_box'] = json.dumps(row['bounding_box'])
            mock_table_data.extend(df_dict)
        mock_table.add.side_effect = mock_add

        # Mock to_pandas().to_dict('records') chain
        # Return a copy to avoid unexpected modifications during tests
        mock_table.to_pandas.return_value.to_dict.side_effect = lambda *args, **kwargs: [item.copy() for item in mock_table_data]

        # Mock search method
        mock_query_builder = MagicMock()
        mock_table.search.return_value = mock_query_builder
        mock_query_builder.where.return_value = mock_query_builder

        # This will store the limit value set by the test
        mock_query_builder.limit_value = None 
        def mock_limit(k):
            mock_query_builder.limit_value = k
            return mock_query_builder
        mock_query_builder.limit.side_effect = mock_limit

        # Mock to_list to simulate search and limit
        def mock_to_list():
            # For search tests, we primarily care about the limit for now.
            # A more robust mock would simulate vector similarity search,
            # but for these tests, simply returning limited mock_table_data is sufficient.
            if mock_query_builder.limit_value is not None:
                return [item.copy() for item in mock_table_data[:mock_query_builder.limit_value]]
            return [item.copy() for item in mock_table_data]
        mock_query_builder.to_list.side_effect = mock_to_list

        # Mock table_names() to reflect whether tables exist based on what's been "created"
        mock_db.table_names.side_effect = lambda: _mock_created_tables

        # Mock create_table
        def mock_create_table(name, *args, **kwargs):
            if name not in _mock_created_tables:
                _mock_created_tables.append(name)
        mock_db.create_table.side_effect = mock_create_table

        # Mock delete method
        def mock_delete(where):
            deleted_count = 0
            new_table_data = []
            
            # Extract face_id or member_id from where clause
            target_face_id = None
            target_member_id = None
            
            if "face_id =" in where:
                target_face_id = where.split("=")[-1].strip().strip("'")
            elif "member_id =" in where:
                target_member_id = where.split("=")[-1].strip().strip("'")
            # For simplicity, if no specific face_id or member_id, assume deletion applies to all in mock_table_data
            # This is a simplification and might need refinement for more complex 'where' clauses.

            for item in mock_table_data:
                should_delete = False
                if target_face_id and item.get("face_id") == target_face_id:
                    should_delete = True
                elif target_member_id and item.get("member_id") == target_member_id:
                    should_delete = True
                elif not target_face_id and not target_member_id: # If neither specified, delete all matching family_id (implicitly)
                    should_delete = True # This needs to be more robust if multiple family_ids are in mock_table_data

                if should_delete:
                    deleted_count += 1
                else:
                    new_table_data.append(item)
            
            mock_table_data[:] = new_table_data # Update in place
            return deleted_count

        mock_table.delete.side_effect = mock_delete

        # Mock update method
        def mock_update(where, **kwargs):
            updated_count = 0
            target_face_id = None
            if "face_id =" in where:
                target_face_id = where.split("=")[-1].strip().strip("'")
            
            changes = kwargs
            
            for item in mock_table_data:
                if target_face_id and item.get("face_id") == target_face_id:
                    for key, value in changes.items():
                        # Ensure bounding_box is a JSON string when updating mock_table_data
                        if key == "bounding_box" and isinstance(value, dict):
                            item[key] = json.dumps(value)
                        else:
                            item[key] = value
                    updated_count += 1
            return updated_count

        mock_table.update.side_effect = mock_update
        
        service = FaceLanceDBService() # Instantiate after mocks are set up
        yield service
        # Cleanup is handled by cleanup_lancedb fixture

# The mock_embedding_service is not directly used by FaceLanceDBService
# so we can remove it or keep it if other tests use it. For now, we'll keep the mock
# but it won't be passed to face_lancedb_service_instance.
@pytest.fixture
def mock_embedding_service():
    """Mocks the embedding_service for consistent test results."""
    # This mock is for app.core.embeddings.embedding_service, not app.core.lancedb.embedding_service
    # which no longer exists. We will need to adjust this if app.core.face_embeddings.face_embedding_service
    # is mocked.
    with patch('app.core.embeddings.embedding_service') as mock: # Patch the correct embedding service
        mock.embed_query.return_value = [0.1] * FACE_EMBEDDING_DIMENSIONS
        yield mock

@pytest.mark.asyncio
async def test_add_face_data(face_lancedb_service_instance, mock_embedding_service):
    service = face_lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id = str(uuid.uuid4())
    
    expected_bbox = {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1}

    face_data = {
        "family_id": family_id,
        "member_id": member_id,
        "face_id": face_id,
        "embedding": [0.2] * FACE_EMBEDDING_DIMENSIONS,
        "bounding_box": expected_bbox,
        "confidence": 0.99,
        "thumbnail_url": "http://example.com/thumb.jpg",
        "original_image_url": "http://example.com/original.jpg",
        "emotion": "happy",
        "emotion_confidence": 0.9,
        "vector_db_id": None,
        "is_vector_db_synced": True
    }
    
    original_embedding = face_data['embedding']
    
    await service.add_face_data(family_id, [face_data])
    
    table_name = service._get_face_table_name(family_id)
    table = service.db.open_table(table_name)
    
    results = table.to_pandas().to_dict('records')
    assert len(results) == 1
    assert results[0]['face_id'] == face_id
    assert results[0]['member_id'] == member_id
    assert json.loads(results[0]['bounding_box']) == expected_bbox
    assert results[0]['confidence'] == face_data['confidence']
    np.testing.assert_array_almost_equal(list(results[0]['vector']), original_embedding) # Use numpy for robust comparison

@pytest.mark.asyncio
async def test_add_face_data_no_embedding_raises_error(face_lancedb_service_instance):
    service = face_lancedb_service_instance
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
async def test_search_faces_by_embedding(face_lancedb_service_instance, mock_embedding_service):
    service = face_lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id_1 = str(uuid.uuid4())
    member_id_2 = str(uuid.uuid4())
    face_id_1 = str(uuid.uuid4())
    face_id_2 = str(uuid.uuid4())
    face_id_3 = str(uuid.uuid4())
    
    # Add some dummy face data
    face_data_1 = {
        "family_id": family_id, "member_id": member_id_1, "face_id": face_id_1,
        "embedding": [0.1] * FACE_EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_2 = {
        "family_id": family_id, "member_id": member_id_1, "face_id": face_id_2,
        "embedding": [0.15] * FACE_EMBEDDING_DIMENSIONS, # Slightly different
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url2", "original_image_url": "orig_url2",
        "emotion": "happy", "emotion_confidence": 0.9, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_3 = {
        "family_id": family_id, "member_id": member_id_2, "face_id": face_id_3,
        "embedding": [0.8] * FACE_EMBEDDING_DIMENSIONS, # Very different
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url3", "original_image_url": "orig_url3",
        "emotion": "sad", "emotion_confidence": 0.7, "vector_db_id": None, "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data_1, face_data_2, face_data_3])
    
    query_embedding = [0.11] * FACE_EMBEDDING_DIMENSIONS # Close to face_data_1 and face_data_2
    results = await service.search_faces(family_id, query_embedding, top_k=2)
    
    assert len(results) == 2
    # The exact order and distance might vary slightly based on LanceDB's internal
    # indexing, but face_id_1 and face_id_2 should be the top results.
    result_face_ids = {res['face_id'] for res in results}
    assert uuid.UUID(face_id_1) in result_face_ids
    assert uuid.UUID(face_id_2) in result_face_ids
    assert uuid.UUID(face_id_3) not in result_face_ids
    
    # Test with member_id filter
    results_filtered = await service.search_faces(family_id, query_embedding, member_id=member_id_1, top_k=1)
    assert len(results_filtered) == 1
    assert results_filtered[0]['member_id'] == uuid.UUID(member_id_1) # Convert to UUID for assertion

@pytest.mark.asyncio
async def test_delete_face_data_by_face_id(face_lancedb_service_instance):
    service = face_lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id_to_delete = str(uuid.uuid4())
    face_id_to_keep = str(uuid.uuid4())
    
    face_data_to_delete = {
        "family_id": family_id, "member_id": member_id, "face_id": face_id_to_delete,
        "embedding": [0.1] * FACE_EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_to_keep = {
        "family_id": family_id, "member_id": member_id, "face_id": face_id_to_keep,
        "embedding": [0.2] * FACE_EMBEDDING_DIMENSIONS,
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
async def test_delete_face_data_by_member_id(face_lancedb_service_instance):
    service = face_lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id_to_delete = str(uuid.uuid4())
    member_id_to_keep = str(uuid.uuid4())
    
    face_data_1 = {
        "family_id": family_id, "member_id": member_id_to_delete, "face_id": str(uuid.uuid4()),
        "embedding": [0.1] * FACE_EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_2 = {
        "family_id": family_id, "member_id": member_id_to_delete, "face_id": str(uuid.uuid4()),
        "embedding": [0.15] * FACE_EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url2", "original_image_url": "orig_url2",
        "emotion": "happy", "emotion_confidence": 0.9, "vector_db_id": None, "is_vector_db_synced": True
    }
    face_data_3 = {
        "family_id": family_id, "member_id": member_id_to_keep, "face_id": str(uuid.uuid4()),
        "embedding": [0.8] * FACE_EMBEDDING_DIMENSIONS,
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
async def test_update_face_data(face_lancedb_service_instance):
    service = face_lancedb_service_instance
    family_id = str(uuid.uuid4())
    member_id = str(uuid.uuid4())
    face_id_to_update = str(uuid.uuid4())
    
    face_data = {
        "family_id": family_id, "member_id": member_id, "face_id": face_id_to_update,
        "embedding": [0.1] * FACE_EMBEDDING_DIMENSIONS,
        "bounding_box": {"x_min": 0, "y_min": 0, "x_max": 1, "y_max": 1},
        "confidence": 0.9, "thumbnail_url": "url1", "original_image_url": "orig_url1",
        "emotion": "neutral", "emotion_confidence": 0.8, "vector_db_id": None, "is_vector_db_synced": True
    }
    
    await service.add_face_data(family_id, [face_data])
    
    update_info = {
        "confidence": 0.95,
        "emotion": "angry"
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
    assert results[0]['member_id'] == member_id # Should remain unchanged
