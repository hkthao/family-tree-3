import pytest
from unittest.mock import AsyncMock, Mock, patch
import httpx
import openai
from httpx import AsyncClient, ASGITransport

from app.main import app
from app.llm.ollama import OllamaLLM
from app.llm.openai import OpenAILLM
from app.schemas import EmbeddingRequest, EmbeddingResponse, EmbeddingData, EmbeddingUsage

@pytest.fixture
def sample_embedding_request():
    return {
        "model": "ollama:nomic-embed-text",
        "input": "This is a test sentence.",
        "encoding_format": "float"
    }

@pytest.fixture
def mock_ollama_embedding_response_payload():
    return {
        "embedding": [0.1, 0.2, 0.3, 0.4]
    }

@pytest.fixture
def mock_openai_embedding_response():
    class MockOpenAIEmbeddingData:
        def __init__(self, embedding_vector):
            self.embedding = embedding_vector
            self.object = "embedding"
            self.index = 0

    class MockOpenAIEmbeddingUsage:
        def __init__(self, prompt_tokens, total_tokens):
            self.prompt_tokens = prompt_tokens
            self.total_tokens = total_tokens

    class MockOpenAIEmbeddingCompletion:
        def __init__(self, embedding_vector, model_name, prompt_tokens, total_tokens):
            self.data = [MockOpenAIEmbeddingData(embedding_vector)]
            self.model = model_name
            self.object = "list"
            self.usage = MockOpenAIEmbeddingUsage(prompt_tokens, total_tokens)

    return MockOpenAIEmbeddingCompletion(
        embedding_vector=[0.5, 0.6, 0.7, 0.8],
        model_name="text-embedding-ada-002",
        prompt_tokens=5,
        total_tokens=5
    )


# --- Test OllamaLLM.embed ---
@pytest.mark.asyncio
async def test_ollama_llm_embed_success(mocker, mock_ollama_embedding_response_payload):
    mock_response = AsyncMock()
    mock_response.status_code = 200
    mock_response.json = AsyncMock(return_value=mock_ollama_embedding_response_payload)
    mock_response.raise_for_status = Mock()
    mocker.patch.object(httpx.AsyncClient, "post", new_callable=AsyncMock, return_value=mock_response)

    ollama_llm = OllamaLLM()
    response = await ollama_llm.embed(
        model="ollama:nomic-embed-text",
        input_text="Test embedding text"
    )

    assert httpx.AsyncClient.post.called
    assert response["data"][0]["embedding"] == [0.1, 0.2, 0.3, 0.4]
    assert response["object"] == "list"
    assert response["model"] == "ollama:nomic-embed-text"


@pytest.mark.asyncio
async def test_ollama_llm_embed_http_error(mocker):
    mock_response = AsyncMock()
    mock_response.status_code = 500
    mock_response.json = AsyncMock(return_value={"detail": "Internal Server Error"})
    mock_response.raise_for_status = Mock(
        side_effect=httpx.HTTPStatusError(
            "error", request=httpx.Request("POST", "/"), response=httpx.Response(500, request=httpx.Request("POST", "/"))
        )
    )
    mocker.patch.object(httpx.AsyncClient, "post", new_callable=AsyncMock, return_value=mock_response)

    ollama_llm = OllamaLLM()
    with pytest.raises(httpx.HTTPStatusError):
        await ollama_llm.embed(
            model="ollama:nomic-embed-text",
            input_text="Test embedding text"
        )


# --- Test OpenAILLM.embed ---
@pytest.mark.asyncio
async def test_openai_llm_embed_success(mocker, mock_openai_embedding_response):
    # Mock the AsyncOpenAI.__init__ method to bypass actual instantiation logic entirely
    mocker.patch("openai.AsyncOpenAI.__init__", return_value=None)
    
    # Now, manually set up the 'client' attribute of the OpenAILLM instance
    openai_llm = OpenAILLM()
    
    # Create a mock object that mimics client.embeddings and then assign it
    mock_embeddings = AsyncMock()
    mock_embeddings.create.return_value = mock_openai_embedding_response
    
    # Ensure openai_llm.client points to a mock with an embeddings structure
    openai_llm.client = Mock() # Make openai_llm.client a generic mock
    openai_llm.client.embeddings = mock_embeddings # Assign our mock_embeddings to it

    response = await openai_llm.embed(
        model="openai:text-embedding-ada-002",
        input_text="Test embedding text"
    )
    
    mock_embeddings.create.assert_called_once()
    assert response["data"][0]["embedding"] == [0.5, 0.6, 0.7, 0.8]
    assert response["object"] == "list"
    assert response["model"] == "openai:text-embedding-ada-002"
    assert response["usage"]["prompt_tokens"] == 5


@pytest.mark.asyncio
async def test_openai_llm_embed_api_error(mocker):
    # Mock the AsyncOpenAI.__init__ method
    mocker.patch("openai.AsyncOpenAI.__init__", return_value=None)

    openai_llm = OpenAILLM()
    
    mock_embeddings = AsyncMock()
    openai_llm.client = Mock()
    openai_llm.client.embeddings = mock_embeddings

    error_request = httpx.Request("POST", "http://api.openai.com/v1/embeddings")
    mock_embeddings.create.side_effect = openai.APIError(
        "API Error message",
        request=error_request,
        body={"error": "details"}
    )

    with pytest.raises(openai.APIError):
        await openai_llm.embed(
            model="openai:text-embedding-ada-002",
            input_text="Test embedding text"
        )


# --- Test API endpoint /v1/embeddings ---
@pytest.mark.asyncio
async def test_api_embeddings_ollama_success(mocker, sample_embedding_request):
    mock_ollama_embed = mocker.patch("app.llm.ollama.OllamaLLM.embed", new_callable=AsyncMock)
    mock_ollama_embed.return_value = EmbeddingResponse(
        data=[EmbeddingData(embedding=[0.1, 0.2, 0.3])],
        model="ollama:nomic-embed-text",
        usage=EmbeddingUsage(prompt_tokens=4, total_tokens=4)
    ).model_dump()

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/embeddings", json=sample_embedding_request)

    assert response.status_code == 200
    assert response.json()["data"][0]["embedding"] == [0.1, 0.2, 0.3]
    mock_ollama_embed.assert_called_once()
    args, kwargs = mock_ollama_embed.call_args
    assert kwargs["model"] == sample_embedding_request["model"]
    assert kwargs["input_text"] == sample_embedding_request["input"]


@pytest.mark.asyncio
async def test_api_embeddings_openai_success(mocker, sample_embedding_request):
    mock_openai_embed = mocker.patch("app.llm.openai.OpenAILLM.embed", new_callable=AsyncMock)
    mock_openai_embed.return_value = EmbeddingResponse(
        data=[EmbeddingData(embedding=[0.5, 0.6, 0.7])],
        model="openai:text-embedding-ada-002",
        usage=EmbeddingUsage(prompt_tokens=4, total_tokens=4)
    ).model_dump()

    sample_embedding_request["model"] = "openai:text-embedding-ada-002"

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/embeddings", json=sample_embedding_request)

    assert response.status_code == 200
    assert response.json()["data"][0]["embedding"] == [0.5, 0.6, 0.7]
    mock_openai_embed.assert_called_once()
    args, kwargs = mock_openai_embed.call_args
    assert kwargs["model"] == sample_embedding_request["model"]
    assert kwargs["input_text"] == sample_embedding_request["input"]


@pytest.mark.asyncio
async def test_api_embeddings_invalid_model_prefix(sample_embedding_request):
    sample_embedding_request["model"] = "unknown:embed-model"

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/embeddings", json=sample_embedding_request)

    assert response.status_code == 400
    assert "Invalid model specified" in response.json()["detail"]


@pytest.mark.asyncio
async def test_api_embeddings_backend_error(mocker, sample_embedding_request):
    mocker.patch("app.llm.ollama.OllamaLLM.embed", new_callable=AsyncMock, side_effect=Exception("Ollama embed error"))

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/embeddings", json=sample_embedding_request)

    assert response.status_code == 500
    assert "An error occurred while communicating with the ollama embeddings backend." in response.json()["detail"]