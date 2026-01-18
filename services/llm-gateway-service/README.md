# LLM Gateway Service

This is a Python microservice acting as an OpenAI-style LLM Gateway. It allows routing chat completion requests to either an Ollama local instance or OpenAI's cloud API based on the model prefix provided in the request.

## Features

- **OpenAI-compatible API**: Provides a `/v1/chat/completions` endpoint.
- **Backend Routing**: Automatically routes requests to Ollama (local) or OpenAI (cloud) based on the `model` field (e.g., `ollama:qwen2.5`, `openai:gpt-4.1-mini`).
- **Configurable**: Uses environment variables for API keys and URLs.
- **Asynchronous**: Built with FastAPI and httpx for efficient I/O operations.

## Tech Stack

- Python 3.10+
- FastAPI
- httpx (async)
- pydantic
- uvicorn
- openai (Python SDK)
- python-dotenv
- pydantic-settings

## Directory Structure

```
llm-gateway-service/
├── app/
│   ├── main.py             # FastAPI application entry point
│   ├── api/
│   │   └── chat.py         # Chat completion API endpoint logic
│   ├── llm/
│   │   ├── base.py         # Base interface for LLM implementations
│   │   ├── ollama.py       # Ollama LLM implementation
│   │   └── openai.py       # OpenAI LLM implementation
│   ├── schemas/
│   │   └── chat.py         # Pydantic models for request/response
│   ├── config.py           # Configuration settings
│   └── prompt_utils.py     # (Currently empty, as per requirements for forwarding messages)
├── requirements.txt        # Python dependencies
├── .env                    # Environment variables (e.g., API keys, URLs)
└── README.md               # This README file
```

## Getting Started

### Prerequisites

- Python 3.10+
- Docker (for running Ollama locally, if desired)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/family-tree-3.git # (Assuming this is part of a larger project)
    cd family-tree-3/services/llm-gateway-service
    ```
2.  **Create a virtual environment and install dependencies:**
    ```bash
    python3 -m venv .venv
    source .venv/bin/activate
    pip install -r requirements.txt
    ```

### Configuration

Create a `.env` file in the `llm-gateway-service/` directory with your configuration:

```
OLLAMA_BASE_URL=http://localhost:11434
OPENAI_API_KEY=sk-your-openai-key-here
DEFAULT_TEMPERATURE=0.0
DEFAULT_MAX_TOKENS=512
```

-   `OLLAMA_BASE_URL`: The URL where your Ollama instance is running.
-   `OPENAI_API_KEY`: Your OpenAI API key.
-   `DEFAULT_TEMPERATURE`: Default sampling temperature for models.
-   `DEFAULT_MAX_TOKENS`: Default maximum tokens to generate.

### Running the Service

```bash
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

The API documentation will be available at `http://localhost:8000/docs`.

### Running Ollama (Optional, for local LLM)

If you want to use Ollama locally, ensure it's running. You can typically run it via Docker:

```bash
docker run -d -v ollama:/root/.ollama -p 11434:11434 --name ollama ollama/ollama
```

Then, pull a model, e.g., Qwen 2.5:

```bash
docker exec -it ollama ollama run qwen2.5
```

*(You might need to let it download completely then `Ctrl+C` to exit the interactive mode).*

### Testing the API

You can test the API using `curl`.

#### Test with Ollama

Ensure Ollama is running and the specified model is downloaded.

```bash
curl http://localhost:8000/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "ollama:qwen2.5",
    "messages": [
      {"role": "system", "content": "You are a helpful assistant."},
      {"role": "user", "content": "Tóm tắt thông tin về Cao Tải"}
    ],
    "temperature": 0,
    "max_tokens": 100
  }'
```

#### Test with OpenAI

Ensure `OPENAI_API_KEY` is set correctly in your `.env` file.

```bash
curl http://localhost:8000/v1/chat/completions \
  -H "Content-Type: application/json" \
  -d '{
    "model": "openai:gpt-3.5-turbo",
    "messages": [
      {"role": "system", "content": "You are a helpful assistant."},
      {"role": "user", "content": "Explain quantum physics in simple terms."}
    ],
    "temperature": 0.7,
    "max_tokens": 150
  }'
```

## Development

### Running tests

To run the unit tests, make sure you have activated your virtual environment and installed all dependencies, including the development dependencies (`pytest`, `pytest-asyncio`, `pytest-mock`).

```bash
# From the llm-gateway-service directory
pytest tests/unit
```

### Linting & Formatting

*(Add linting/formatting instructions here)
