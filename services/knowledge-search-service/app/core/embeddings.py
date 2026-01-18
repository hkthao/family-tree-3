from sentence_transformers import SentenceTransformer
from loguru import logger

from ..config import EMBEDDING_MODEL_NAME


class EmbeddingService:
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(EmbeddingService, cls).__new__(cls)
            cls._instance.model = None  # Initialize model to None
        return cls._instance

    def _load_model(self):
        """Loads the sentence transformer model once if not already loaded."""
        if self.model is None:
            logger.info(f"Loading embedding model: {EMBEDDING_MODEL_NAME}...")
            self.model = SentenceTransformer(EMBEDDING_MODEL_NAME)
            logger.info(f"Embedding model {EMBEDDING_MODEL_NAME} loaded.")

    def embed_query(self, query: str):
        """Embeds a single query string."""
        self._load_model()  # Ensure model is loaded before embedding
        return self.model.encode(query).tolist()


# Initialize the embedding service globally
embedding_service = EmbeddingService()
