import pytest
from httpx import AsyncClient, ASGITransport
from unittest.mock import AsyncMock, patch

from app.main import app
from app.schemas import ChatMessage # Corrected import
from app.config import settings

@pytest.fixture
def sample_chat_request():
    return {
        "model": "ollama:test-model",
        "messages": [
            {"role": "system", "content": "You are a test assistant."},
            {"role": "user", "content": "Say hello!"}
        ],
        "temperature": 0.5,
        "max_tokens": 50,
        "stream": False
    }

@pytest.mark.asyncio
async def test_chat_completion_ollama_success(mocker, sample_chat_request):
    mock_ollama_chat = mocker.patch("app.llm.ollama.OllamaLLM.chat", new_callable=AsyncMock)
    mock_ollama_chat.return_value = {
        "id": "chatcmpl-ollama-mock",
        "object": "chat.completion",
        "choices": [
            {
                "index": 0,
                "message": {"role": "assistant", "content": "Hello from mocked Ollama!"},
                "finish_reason": "stop"
            }
        ]
    }

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/chat/completions", json=sample_chat_request)

    assert response.status_code == 200
    assert response.json()["choices"][0]["message"]["content"] == "Hello from mocked Ollama!"
    mock_ollama_chat.assert_called_once()
    args, kwargs = mock_ollama_chat.call_args
    assert kwargs["model"] == sample_chat_request["model"]
    assert kwargs["messages"] == sample_chat_request["messages"] # Corrected assertion
    assert kwargs["temperature"] == sample_chat_request["temperature"]
    assert kwargs["max_tokens"] == sample_chat_request["max_tokens"]
    assert kwargs["stream"] == sample_chat_request["stream"]


@pytest.mark.asyncio
async def test_chat_completion_openai_success(mocker, sample_chat_request):
    mocker.patch("app.config.settings.OPENAI_API_KEY", "sk-mock-key") # Mock API key
    mock_openai_chat = mocker.patch("app.llm.openai.OpenAILLM.chat", new_callable=AsyncMock)
    mock_openai_chat.return_value = {
        "id": "chatcmpl-openai-mock",
        "object": "chat.completion",
        "choices": [
            {
                "index": 0,
                "message": {"role": "assistant", "content": "Hello from mocked OpenAI!"},
                "finish_reason": "stop"
            }
        ]
    }
    sample_chat_request["model"] = "openai:gpt-3.5-turbo"

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/chat/completions", json=sample_chat_request)

    assert response.status_code == 200
    assert response.json()["choices"][0]["message"]["content"] == "Hello from mocked OpenAI!"
    mock_openai_chat.assert_called_once()
    args, kwargs = mock_openai_chat.call_args
    assert kwargs["model"] == sample_chat_request["model"]
    assert kwargs["messages"] == sample_chat_request["messages"] # Corrected assertion


@pytest.mark.asyncio
async def test_chat_completion_invalid_model_prefix(sample_chat_request):
    sample_chat_request["model"] = "unknown:model"

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/chat/completions", json=sample_chat_request)

    assert response.status_code == 400
    assert "Invalid model specified" in response.json()["detail"]


@pytest.mark.asyncio
async def test_chat_completion_ollama_backend_error(mocker, sample_chat_request):
    mocker.patch("app.llm.ollama.OllamaLLM.chat", new_callable=AsyncMock, side_effect=Exception("Ollama error"))

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/chat/completions", json=sample_chat_request)

    assert response.status_code == 500
    assert "An error occurred while communicating with the ollama backend." in response.json()["detail"]


@pytest.mark.asyncio
async def test_chat_completion_openai_backend_error(mocker, sample_chat_request):
    mocker.patch("app.config.settings.OPENAI_API_KEY", "sk-mock-key") # Mock API key
    mocker.patch("app.llm.openai.OpenAILLM.chat", new_callable=AsyncMock, side_effect=Exception("OpenAI error"))
    sample_chat_request["model"] = "openai:gpt-3.5-turbo"

    async with AsyncClient(transport=ASGITransport(app=app), base_url="http://test") as client:
        response = await client.post("/v1/chat/completions", json=sample_chat_request)

    assert response.status_code == 500
    assert "An error occurred while communicating with the openai backend." in response.json()["detail"]