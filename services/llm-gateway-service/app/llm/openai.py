import logging
from typing import List, Dict, Any

from openai import AsyncOpenAI
from app.llm.base import BaseLLM
from app.config import settings
from app.schemas import ( # Corrected import
    ChatMessage, ChatCompletionResponse, ChatCompletionChoice, ChatCompletionMessage, 
    EmbeddingResponse, EmbeddingData, EmbeddingUsage
)

logger = logging.getLogger(__name__)

class OpenAILLM(BaseLLM):
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY, base_url=settings.OPENAI_BASE_URL)

    async def chat(self, model: str, messages: List[Dict[str, Any]], temperature: float, max_tokens: int, stream: bool) -> Dict[str, Any]:
        """
        Interacts with the OpenAI API to get chat completions.
        Args:
            model: The OpenAI model name (e.g., "gpt-4.1-mini").
            messages: A list of chat messages.
            temperature: Sampling temperature.
            max_tokens: Max tokens to generate.
            stream: Whether to stream the response.
        Returns:
            A dictionary representing the LLM's response, compatible with OpenAI's chat completion format.
        """
        openai_model_name = model.split("openai:", 1)[1] if model.startswith("openai:") else model

        logger.info(f"OpenAI Chat Request: Model={openai_model_name}, Temp={temperature}, MaxTokens={max_tokens}")
        try:
            # The OpenAI SDK handles the streaming part if stream=True
            # For this gateway, we are currently only returning non-streaming responses.
            if stream:
                logger.warning("Streaming is not fully implemented for OpenAI in this gateway's current response parsing. Forcing non-streaming.")
                stream = False # Override to false as per current gateway requirements.

            response = await self.client.chat.completions.create(
                model=openai_model_name,
                messages=messages,
                temperature=temperature,
                max_tokens=max_tokens,
                stream=stream,
            )

            if stream:
                content_accumulator = ""
                async for chunk in response:
                    if chunk.choices and chunk.choices[0].delta.content:
                        content_accumulator += chunk.choices[0].delta.content
                content = content_accumulator
            else:
                content = response.choices[0].message.content

            # Wrap OpenAI response into OpenAI-style response
            openai_message = ChatCompletionMessage(role="assistant", content=content)
            openai_choice = ChatCompletionChoice(index=0, message=openai_message, finish_reason="stop")
            openai_response = ChatCompletionResponse(choices=[openai_choice])
            
            return openai_response.model_dump()

        except Exception as e:
            logger.error(f"OpenAI Chat API error: {e}")
            raise

    async def embed(self, model: str, input_text: str) -> Dict[str, Any]:
        """
        Interacts with the OpenAI API to get embeddings.
        Args:
            model: The OpenAI embedding model name (e.g., "text-embedding-ada-002").
            input_text: The text string to embed.
        Returns:
            A dictionary representing the embedding response, compatible with OpenAI's embedding format.
        """
        openai_model_name = model.split("openai:", 1)[1] if model.startswith("openai:") else model

        logger.info(f"OpenAI Embed Request: Model={openai_model_name}, Text='{input_text[:50]}...'")
        try:
            response = await self.client.embeddings.create(
                model=openai_model_name,
                input=input_text
            )

            embedding_vector = response.data[0].embedding
            prompt_tokens = response.usage.prompt_tokens
            total_tokens = response.usage.total_tokens

            embedding_data = EmbeddingData(embedding=embedding_vector)
            embedding_usage = EmbeddingUsage(prompt_tokens=prompt_tokens, total_tokens=total_tokens)
            
            embedding_response = EmbeddingResponse(
                data=[embedding_data],
                model=model,
                usage=embedding_usage
            )
            return embedding_response.model_dump()

        except Exception as e:
            logger.error(f"OpenAI Embed API error: {e}")
            raise