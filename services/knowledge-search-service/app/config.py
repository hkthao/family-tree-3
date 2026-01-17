import os

# LanceDB configuration
LANCEDB_PATH = os.getenv("LANCEDB_PATH", "lancedb")

# Embedding model configuration
EMBEDDING_MODEL_NAME = os.getenv("EMBEDDING_MODEL_NAME", "all-MiniLM-L6-v2")
EMBEDDING_DIMENSIONS = 384
