import pytest
from unittest.mock import MagicMock, patch
import numpy as np

# Import the module under test
# Need to adjust import based on the project structure
# Assuming the tests are run from the project root or similar,
# and app is a package.
from app.core.embeddings import EmbeddingService, embedding_service


@pytest.fixture(autouse=True)
def mock_embedding_dependencies(mocker):
    """
    Fixture to mock external dependencies for EmbeddingService.
    - Mocks SentenceTransformer to control its behavior.
    - Mocks loguru.logger to prevent logging output during tests.
    - Resets the singleton instance of EmbeddingService for isolation.
    """
    # Mock SentenceTransformer
    mock_sentence_transformer_instance = MagicMock()
    # Configure the encode method to return a predictable numpy array
    mock_sentence_transformer_instance.encode.return_value = np.array([0.1, 0.2, 0.3])
    mocker.patch(
        "app.core.embeddings.SentenceTransformer",
        return_value=mock_sentence_transformer_instance
    )

    # Mock loguru logger
    mocker.patch("app.core.embeddings.logger")

    # Reset the singleton instance to ensure test isolation
    # We patch the _instance attribute of the class directly
    mocker.patch.object(EmbeddingService, "_instance", None)
    # Ensure the model attribute is also reset if a model was loaded in a previous test
    # The __new__ method initializes model to None, but subsequent calls to _load_model
    # would set it. Patching _instance to None effectively resets the singleton state.

    yield mock_sentence_transformer_instance # Provide the mock instance for specific assertions if needed


def test_embed_query_success(mock_embedding_dependencies):
    """
    Test case 1: Successful embedding of a query.
    - Verifies that SentenceTransformer.encode is called.
    - Verifies that the correct embedding list is returned.
    """
    service = EmbeddingService()
    query = "test query"
    expected_embedding = [0.1, 0.2, 0.3]

    result = service.embed_query(query)

    mock_sentence_transformer_instance = mock_embedding_dependencies
    mock_sentence_transformer_instance.encode.assert_called_once_with(query)
    assert result == expected_embedding


def test_embed_query_model_load_failure(mocker):
    """
    Test case 2: Model loading fails during embed_query.
    - Verifies that an exception during SentenceTransformer initialization is propagated.
    """
    # Simulate an error during SentenceTransformer initialization
    mocker.patch(
        "app.core.embeddings.SentenceTransformer",
        side_effect=Exception("Failed to load model")
    )
    mocker.patch.object(EmbeddingService, "_instance", None) # Ensure fresh instance attempt

    service = EmbeddingService()
    query = "another query"

    with pytest.raises(Exception, match="Failed to load model"):
        service.embed_query(query)


def test_embed_query_encode_failure(mock_embedding_dependencies):
    """
    Test case 3: SentenceTransformer.encode method fails.
    - Verifies that an exception during the encode method is propagated.
    """
    mock_sentence_transformer_instance = mock_embedding_dependencies
    # Configure the encode method to raise an exception
    mock_sentence_transformer_instance.encode.side_effect = Exception("Encoding error")

    service = EmbeddingService()
    query = "failing query"

    with pytest.raises(Exception, match="Encoding error"):
        service.embed_query(query)
