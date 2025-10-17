# Frontend Environment Variables
VITE_USE_MOCK=false
VITE_API_BASE_URL="/api"
VITE_AUTH0_DOMAIN="dev-g76tq00gicwdzk3z.us.auth0.com"
VITE_AUTH0_CLIENT_ID="v4jSe5QR4Uj6ddoBBMHNtaDNHwv8UzQN"
VITE_AUTH0_AUDIENCE="http://localhost:5000"

# Backend Environment Variables (for Docker)
AIChatSettings__Provider="Local"
AIChatSettings__MaxTokensPerRequest=500
AIChatSettings__DailyUsageLimit=10000
AIChatSettings__ScoreThreshold=0
AIChatSettings__Providers__Gemini__ApiKey="YOUR_GEMINI_API_KEY"
AIChatSettings__Providers__Gemini__Model="gemini-pro"
AIChatSettings__Providers__OpenAI__ApiKey="YOUR_OPENAI_API_KEY"
AIChatSettings__Providers__OpenAI__Model="gpt-3.5-turbo"
AIChatSettings__Providers__Local__ApiUrl="http://localhost:11434/api/chat"
AIChatSettings__Providers__Local__Model="llama3:8b"

EmbeddingSettings__Provider="Local"
EmbeddingSettings__OpenAI__ApiKey="YOUR_OPENAI_API_KEY"
EmbeddingSettings__OpenAI__Model="text-embedding-ada-002"
EmbeddingSettings__Cohere__ApiKey="YOUR_COHERE_API_KEY"
EmbeddingSettings__Cohere__Model="embed-english-v3.0"
EmbeddingSettings__Local__ApiUrl="http://localhost:11434/api/embed"
EmbeddingSettings__Local__Model="llama3:8b"
EmbeddingSettings__Local__MaxTextLength="512"

VectorStoreSettings__Provider="Qdrant"
VectorStoreSettings__TopK=3
VectorStoreSettings__Pinecone__ApiKey="YOUR_PINECONE_API_KEY"
VectorStoreSettings__Pinecone__Host="https://family-tree-ai-assistant-xu854pl.svc.aped-4627-b74a.pinecone.io"
VectorStoreSettings__Pinecone__IndexName="family-tree-ai-assistant"
VectorStoreSettings__Qdrant__Host="706d0a2b-a05e-40f9-bd44-b033c88e68d4.europe-west3-0.gcp.cloud.qdrant.io"
VectorStoreSettings__Qdrant__apiKey="YOUR_QDRANT_API_KEY"
VectorStoreSettings__Qdrant__CollectionName="family-tree-ai-assistant"
VectorStoreSettings__Qdrant__VectorSize="4096"

ConnectionStrings__DefaultConnection="Server=family-tree-mysql;Port=3306;Database=familytree_db;Uid=root;Pwd=root_password;"
UseInMemoryDatabase=false

JwtSettings__Authority="dev-g76tq00gicwdzk3z.us.auth0.com"
JwtSettings__Audience="http://localhost:5000"
JwtSettings__Namespace="https://familytree.com/"

StorageSettings__Provider="Cloudinary"
StorageSettings__BaseUrl="http://localhost:5173"
StorageSettings__Local__LocalStoragePath="wwwroot/uploads"
StorageSettings__Local__BaseUrl="http://localhost:5173"
StorageSettings__Cloudinary__CloudName="dcdjaqq41"
StorageSettings__Cloudinary__ApiKey="YOUR_CLOUDINARY_API_KEY"
StorageSettings__Cloudinary__ApiSecret="YOUR_CLOUDINARY_API_SECRET"
StorageSettings__S3__BucketName="YOUR_S3_BUCKET_NAME"
StorageSettings__S3__AccessKey="YOUR_S3_ACCESS_KEY"
StorageSettings__S3__SecretKey="YOUR_S3_SECRET_KEY"
StorageSettings__S3__Region="YOUR_S3_REGION"

FaceDetectionService__BaseUrl="http://face-service:8000"