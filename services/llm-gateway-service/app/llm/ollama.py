import httpx
import logging
from typing import List, Dict, Any

from app.llm.base import BaseLLM
from app.config import settings
from app.schemas import ( # Corrected import
    ChatMessage, ChatCompletionResponse, ChatCompletionChoice, ChatCompletionMessage, 
    EmbeddingResponse, EmbeddingData, EmbeddingUsage
)

logger = logging.getLogger(__name__)

class OllamaLLM(BaseLLM):
    def __init__(self):
        self.base_url = settings.OLLAMA_BASE_URL
        self.client = httpx.AsyncClient(base_url=self.base_url)

    async def chat(self, model: str, messages: List[Dict[str, Any]], temperature: float, max_tokens: int, stream: bool) -> Dict[str, Any]:
        """
        Interacts with the Ollama API to get chat completions.
        Args:
            model: The Ollama model name (e.g., "qwen2.5").
            messages: A list of chat messages.
            temperature: Sampling temperature.
            max_tokens: Max tokens to generate.
            stream: Whether to stream the response.
        Returns:
            A dictionary representing the LLM's response, compatible with OpenAI's chat completion format.
        """
        ollama_model_name = model.split("ollama:", 1)[1] if model.startswith("ollama:") else model
        
        payload = {
            "model": ollama_model_name,
            "messages": messages,
            "stream": False,  # Force non-streaming for now as per requirements for simplified response
            "options": {
                "temperature": temperature,
                "num_predict": max_tokens, 
            }
        }
        logger.info(f"Ollama Chat Request: Model={ollama_model_name}, Temp={temperature}, MaxTokens={max_tokens}")
        try:
            response = await self.client.post("/api/chat", json=payload, timeout=None)
            response.raise_for_status() # This is a synchronous call on httpx.Response
            ollama_response = response.json() # Remove await

            content = ollama_response.get("message", {}).get("content", "")

            # Wrap Ollama response into OpenAI-style response
            openai_message = ChatCompletionMessage(role="assistant", content=content)
            openai_choice = ChatCompletionChoice(index=0, message=openai_message, finish_reason="stop")
            openai_response = ChatCompletionResponse(choices=[openai_choice])
            
            return openai_response.model_dump()

        except httpx.HTTPStatusError as e:
            logger.error(f"Ollama Chat API error: {e.response.status_code} - {e.response.text}")
            raise
        except httpx.RequestError as e:
            logger.error(f"Ollama Chat network error: {e}")
            raise

    async def embed(self, model: str, input_text: str) -> Dict[str, Any]:
        """
        Interacts with the Ollama API to get embeddings.
        Args:
            model: The Ollama embedding model name (e.g., "nomic-embed-text").
            input_text: The text string to embed.
        Returns:
            A dictionary representing the embedding response, compatible with OpenAI's embedding format.
        """
        ollama_model_name = model.split("ollama:", 1)[1] if model.startswith("ollama:") else model

        payload = {
            "model": ollama_model_name,
            "prompt": input_text,
            "options": {
                "num_predict": -1 # Instruct Ollama to generate embeddings only
            }
        }
        logger.info(f"Ollama Embed Request: Model={ollama_model_name}, Text='{input_text[:50]}...'")
        try:
            response = await self.client.post("/api/embeddings", json=payload, timeout=None)
            response.raise_for_status()
            ollama_response = await response.json()

            embedding_vector = ollama_response.get("embedding", [])
            # Ollama doesn't provide token usage directly for embeddings API in the same format.
            # We can approximate or leave it default as per OpenAI spec.
            # For simplicity, we'll return a default usage.

            embedding_data = EmbeddingData(embedding=embedding_vector)
            embedding_usage = EmbeddingUsage(prompt_tokens=len(input_text.split()), total_tokens=len(input_text.split())) # Simple token count
            
            embedding_response = EmbeddingResponse(
                data=[embedding_data],
                model=model,
                usage=embedding_usage
            )
            return embedding_response.model_dump()

        except httpx.HTTPStatusError as e:
            logger.error(f"Ollama Embed API error: {e.response.status_code} - {e.response.text}")
            raise
        except httpx.RequestError as e:
            logger.error(f"Ollama Embed network error: {e}")
            raise
