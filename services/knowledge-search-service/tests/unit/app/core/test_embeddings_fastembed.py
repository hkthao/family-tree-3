import os
from unittest.mock import patch, MagicMock
import pytest

# Temporarily set the environment variable for testing
# This should ideally be handled by pytest fixtures or test runner config
os.environ["TEXT_EMBEDDING_MODEL_NAME"] = "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
os.environ["TEXT_EMBEDDING_DIMENSIONS"] = "384"

# Import after setting env var to ensure config loads correctly
from app.core.embeddings import EmbeddingService
from app.config import TEXT_EMBEDDING_DIMENSIONS


@pytest.fixture(scope="module")
def embedding_service_instance():
    """Provides a singleton instance of EmbeddingService for tests."""
    # Ensure the model is loaded only once per test module
    service = EmbeddingService()
    # Mock TextEmbedding to avoid downloading models during tests
    with patch('app.core.embeddings.TextEmbedding') as MockTextEmbedding:
        mock_model_instance = MagicMock()
        
        # Create a mock class for numpy array with .tolist() method
        class MockNumpyArray(list):
            def tolist(self):
                return list(self)
        
        mock_model_instance.embed.side_effect = lambda documents: (
            MockNumpyArray([0.1] * TEXT_EMBEDDING_DIMENSIONS) for _ in documents
        ) # Mock embedding logic
        MockTextEmbedding.return_value = mock_model_instance
        
        # Reload the service to ensure the mock is used
        EmbeddingService._model = None
        service._load_model()
        yield service

def test_embedding_service_singleton(embedding_service_instance):
    """Verify that EmbeddingService is a singleton."""
    service1 = EmbeddingService()
    service2 = EmbeddingService()
    assert service1 is service2


def test_embed_query(embedding_service_instance):
    """Test embedding a single query string."""
    query = "Hello world"
    expected_prefix_query = "query: Hello world"
    embedding = embedding_service_instance.embed_query(query)
    
    # Check if TextEmbedding.embed was called with the correct prefix
    embedding_service_instance._model.embed.assert_called_with(documents=[expected_prefix_query])
    assert isinstance(embedding, list)
    assert len(embedding) == 384 # Updated dimension
    
    
def test_embed_documents(embedding_service_instance):
    """Test embedding a list of document strings."""
    documents = ["Doc one", "Doc two", "Doc three"]
    expected_prefix_docs = ["passage: Doc one", "passage: Doc two", "passage: Doc three"]
    
    embeddings = embedding_service_instance.embed_documents(documents)
    
    # Check if TextEmbedding.embed was called with the correct prefixes
    embedding_service_instance._model.embed.assert_called_with(documents=expected_prefix_docs)
    assert isinstance(embeddings, list)
    assert len(embeddings) == len(documents)
    assert all(isinstance(e, list) for e in embeddings)
    assert all(len(e) == 384 for e in embeddings) # Updated dimension
    
    
def test_embedding_dimensions_consistency(embedding_service_instance):
    """Ensure embeddings have consistent dimensions (mocked)."""
    query = "Test query"
    doc = "Test document"
    
    query_embedding = embedding_service_instance.embed_query(query)
    doc_embedding = embedding_service_instance.embed_documents([doc])[0]
    
    assert len(query_embedding) == len(doc_embedding)
    assert len(query_embedding) == 384 # Updated dimension
    
    
    # Clean up the environment variable after tests
def teardown_module(module):
    if "TEXT_EMBEDDING_MODEL_NAME" in os.environ:
        del os.environ["TEXT_EMBEDDING_MODEL_NAME"]
    if "TEXT_EMBEDDING_DIMENSIONS" in os.environ:
        del os.environ["TEXT_EMBEDDING_DIMENSIONS"]