import logging

from fastapi import APIRouter, HTTPException, status
from app.schemas import EmbeddingRequest, EmbeddingResponse
from app.llm.ollama import OllamaLLM
from app.llm.openai import OpenAILLM
from app.config import settings

logger = logging.getLogger(__name__)

router = APIRouter()

# Initialize LLM clients globally to reuse them (same instances as for chat)
ollama_llm_client = OllamaLLM()
openai_llm_client = OpenAILLM()

@router.post("/v1/embeddings", response_model=EmbeddingResponse)
async def create_embeddings(request: EmbeddingRequest):
    """
    Handles embedding requests, routing them to the appropriate LLM backend (Ollama or OpenAI).
    """
    model_name = request.model
    input_text = request.input

    llm_client = None
    llm_provider = None

    if model_name.startswith("ollama:"):
        llm_client = ollama_llm_client
        llm_provider = "ollama"
    elif model_name.startswith("openai:"):
        llm_client = openai_llm_client
        llm_provider = "openai"
    else:
        logger.error(f"Invalid model prefix for embeddings: {model_name}. Must be 'ollama:' or 'openai:'.")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid model specified. Must start with 'ollama:' or 'openai:'."
        )

    logger.info(f"Routing embeddings request to {llm_provider} for model: {model_name}")

    try:
        response_data = await llm_client.embed(
            model=model_name,
            input_text=input_text
        )
        return EmbeddingResponse(**response_data)
    except HTTPException:
        raise # Re-raise if it's already an HTTPException
    except Exception as e:
        logger.error(f"Error processing embeddings with {llm_provider} for model {model_name}: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"An error occurred while communicating with the {llm_provider} embeddings backend."
        )