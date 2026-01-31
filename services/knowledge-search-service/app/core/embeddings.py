from fastembed import TextEmbedding
from loguru import logger
import os
import tempfile

from ..config import TEXT_EMBEDDING_MODEL_NAME


class EmbeddingService:
    _instance = None
    _model = None  # Class-level attribute for the model

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(EmbeddingService, cls).__new__(cls)
            cls._instance._load_model()
        return cls._instance

    def _load_model(self):
        """Loads the embedding model once if not already loaded."""
        if EmbeddingService._model is None:
            logger.info(f"Loading embedding model: {TEXT_EMBEDDING_MODEL_NAME}...")
            # Configure ONNX Runtime to limit CPU threads
            # This can also be set via environment variables like OMP_NUM_THREADS
            # For fastembed, you might control this via onnxruntime options if exposed,
            # or rely on system-wide environment variables.
            # Example: os.environ["OMP_NUM_THREADS"] = "4"
            EmbeddingService._model = TextEmbedding(
                model_name=TEXT_EMBEDDING_MODEL_NAME,
                cache_dir=os.path.join(tempfile.gettempdir(), ".fastembed_cache")  # Use system temp directory
            )
            logger.info(f"Embedding model {TEXT_EMBEDDING_MODEL_NAME} loaded.")

    def embed_query(self, query: str):
        """Embeds a single query string using the 'query:' prefix."""
        # FastEmbed expects a list of texts, even for a single item
        # The embed method returns a list of embeddings (numpy arrays)
        embedding_generator = EmbeddingService._model.embed(documents=[f"query: {query}"])
        first_embedding = next(embedding_generator)
        return first_embedding.tolist()

    def embed_documents(self, documents: list[str]):
        """Embeds a list of document strings using the 'passage:' prefix."""
        # FastEmbed handles batch processing automatically for lists
        prefixed_documents = [f"passage: {doc}" for doc in documents]
        embeddings = EmbeddingService._model.embed(documents=prefixed_documents)
        return [e.tolist() for e in embeddings]  # Convert each embedding to a list


# Initialize the embedding service globally
embedding_service = EmbeddingService()

# Example of how to limit CPU threads for ONNX Runtime if needed
# This should ideally be set before any ONNX Runtime operations
# os.environ["OMP_NUM_THREADS"] = "4"
# logger.info(f"OMP_NUM_THREADS set to: {os.environ.get('OMP_NUM_THREADS')}")
