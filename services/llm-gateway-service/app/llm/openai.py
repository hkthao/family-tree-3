import logging
from typing import List, Dict, Any

from openai import AsyncOpenAI
from app.llm.base import BaseLLM
from app.config import settings
from app.schemas.chat import ChatCompletionResponse, ChatCompletionChoice, ChatCompletionMessage

logger = logging.getLogger(__name__)

class OpenAILLM(BaseLLM):
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)

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

        logger.info(f"OpenAI Request: Model={openai_model_name}, Temp={temperature}, MaxTokens={max_tokens}")
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
                # This block will not be reached due to forcing stream=False above,
                # but kept as a placeholder if streaming is to be supported later.
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
            logger.error(f"OpenAI API error: {e}")
            raise
