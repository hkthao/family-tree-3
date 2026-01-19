import pyarrow as pa
from ..config import TEXT_EMBEDDING_DIMENSIONS, FACE_EMBEDDING_DIMENSIONS


# Schema for general knowledge data in LanceDB
KNOWLEDGE_LANCEDB_SCHEMA = pa.schema([
    pa.field("vector", pa.list_(pa.float32(), TEXT_EMBEDDING_DIMENSIONS)),
    pa.field("family_id", pa.string()),
    pa.field("entity_id", pa.string()), # Unique ID for the entity within the family
    pa.field("type", pa.string()), # e.g., 'family', 'member', 'event', 'document'
    pa.field("visibility", pa.string()), # e.g., 'public', 'private', 'family_only'
    pa.field("name", pa.string()), # Display name of the entity
    pa.field("summary", pa.string()), # Textual summary used for embedding
    pa.field("metadata", pa.string()) # JSON string of all original metadata
])

# Schema for face data in LanceDB
FACE_LANCEDB_SCHEMA = pa.schema([
    pa.field("vector", pa.list_(pa.float32(), FACE_EMBEDDING_DIMENSIONS)),
    pa.field("family_id", pa.string()), # Add family_id here
    pa.field("face_id", pa.string()), # Unique ID for this specific face embedding
    pa.field("member_id", pa.string()), # ID of the family member this face belongs to
    pa.field("bounding_box", pa.string()), # JSON string of bounding box coordinates
    pa.field("confidence", pa.float64()), # Confidence score of face detection/recognition
    pa.field("thumbnail_url", pa.string()), # URL to a thumbnail of the face
    pa.field("original_image_url", pa.string()), # URL to the original image
    pa.field("emotion", pa.string()), # Detected emotion
    pa.field("emotion_confidence", pa.float64()), # Confidence of emotion detection
    pa.field("vector_db_id", pa.string()), # ID used by the external vector database (if applicable)
    pa.field("is_vector_db_synced", pa.bool_()) # Flag indicating sync status with external DB
])
