import logging
from typing import Dict, Any

from fastapi import APIRouter, HTTPException, status
from app.schemas import ChatCompletionRequest, ChatCompletionResponse # Corrected import
from app.llm.ollama import OllamaLLM
from app.llm.openai import OpenAILLM
from app.config import settings

logger = logging.getLogger(__name__)

router = APIRouter()

# Initialize LLM clients globally to reuse them
ollama_llm_client = OllamaLLM()
openai_llm_client = OpenAILLM()

@router.post("/v1/chat/completions", response_model=ChatCompletionResponse)
async def create_chat_completion(request: ChatCompletionRequest):
    """
    Handles chat completion requests, routing them to the appropriate LLM backend (Ollama or OpenAI).
    """
    model_name = request.model
    messages = request.messages
    temperature = request.temperature
    max_tokens = request.max_tokens or settings.DEFAULT_MAX_TOKENS
    stream = request.stream

    llm_client = None
    llm_provider = None

    if model_name.startswith("ollama:"):
        llm_client = ollama_llm_client
        llm_provider = "ollama"
    elif model_name.startswith("openai:"):
        llm_client = openai_llm_client
        llm_provider = "openai"
    else:
        logger.error(f"Invalid model prefix: {model_name}. Must be 'ollama:' or 'openai:'.")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid model specified. Must start with 'ollama:' or 'openai:'."
        )

    logger.info(f"Routing request to {llm_provider} for model: {model_name}")

    try:
        # Convert Pydantic messages to dicts for LLM clients
        messages_dicts = [msg.model_dump() for msg in messages]
        response_data: Dict[str, Any] = await llm_client.chat(
            model=model_name,
            messages=messages_dicts,
            temperature=temperature,
            max_tokens=max_tokens,
            stream=stream
        )
        return ChatCompletionResponse(**response_data)
    except HTTPException:
        raise # Re-raise if it's already an HTTPException
    except Exception as e:
        logger.error(f"Error processing chat completion with {llm_provider} for model {model_name}: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"An error occurred while communicating with the {llm_provider} backend."
        )