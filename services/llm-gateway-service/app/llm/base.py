from abc import ABC, abstractmethod
from typing import List, Dict, Any

class BaseLLM(ABC):
    @abstractmethod
    async def chat(self, model: str, messages: List[Dict[str, Any]], temperature: float, max_tokens: int, stream: bool) -> Dict[str, Any]:
        """
        Abstract method to interact with an LLM.
        Args:
            model: The specific model name (e.g., "qwen2.5", "gpt-4.1-mini").
            messages: A list of chat messages, each with a "role" and "content".
            temperature: Sampling temperature to use.
            max_tokens: The maximum number of tokens to generate.
            stream: Whether to stream the response.
        Returns:
            A dictionary representing the LLM's response, compatible with OpenAI's chat completion format.
        """
        pass
