import os

# LanceDB configuration
LANCEDB_PATH = os.getenv("LANCEDB_PATH", "lancedb")

# Embedding model configuration
TEXT_EMBEDDING_MODEL_NAME = os.getenv("TEXT_EMBEDDING_MODEL_NAME", "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2")
TEXT_EMBEDDING_DIMENSIONS = int(os.getenv("TEXT_EMBEDDING_DIMENSIONS", "384"))
