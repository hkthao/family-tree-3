namespace backend.Infrastructure.AppSetting;

public class AppSettings
{
    // This class will be populated from appsettings.json
    // Add properties here that correspond to your appsettings.json structure
    // For example:
    // public string SomeSetting { get; set; } = null!;
    // public int AnotherSetting { get; set; } = 0;

    public AIChatSettings AIChatSettings { get; set; } = new AIChatSettings();
    public EmbeddingSettings EmbeddingSettings { get; set; } = new EmbeddingSettings();
    public VectorStoreSettings VectorStoreSettings { get; set; } = new VectorStoreSettings();
    public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    public JwtSettings JwtSettings { get; set; } = new JwtSettings();
    public StorageSettings StorageSettings { get; set; } = new StorageSettings();
    public FaceDetectionService FaceDetectionService { get; set; } = new FaceDetectionService();
    public string CORS_ORIGINS { get; set; } = null!;
    public bool UseInMemoryDatabase { get; set; }
}

public class AIChatSettings
{
    public string Provider { get; set; } = null!;
    public int ScoreThreshold { get; set; }
    public GeminiSettings Gemini { get; set; } = new GeminiSettings();
    public OpenAISettings OpenAI { get; set; } = new OpenAISettings();
    public LocalSettings Local { get; set; } = new LocalSettings();
}

public class GeminiSettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Region { get; set; } = null!;
}

public class OpenAISettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Organization { get; set; } = null!;
}

public class LocalSettings
{
    public string ApiUrl { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string MaxTextLength { get; set; } = null!;
}

public class EmbeddingSettings
{
    public string Provider { get; set; } = null!;
    public OpenAISettings OpenAI { get; set; } = new OpenAISettings();
    public CohereSettings Cohere { get; set; } = new CohereSettings();
    public LocalSettings Local { get; set; } = new LocalSettings();
}

public class CohereSettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int MaxTextLength { get; set; }
}

public class VectorStoreSettings
{
    public string Provider { get; set; } = null!;
    public int TopK { get; set; }
    public PineconeSettings Pinecone { get; set; } = new PineconeSettings();
    public QdrantSettings Qdrant { get; set; } = new QdrantSettings();
}

public class PineconeSettings
{
    public string ApiKey { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string IndexName { get; set; } = null!;
}

public class QdrantSettings
{
    public string Host { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
    public string VectorSize { get; set; } = null!;
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; } = null!;
}

public class JwtSettings
{
    public string Authority { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string Namespace { get; set; } = null!;
}

public class StorageSettings
{
    public string Provider { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public LocalStorageSettings Local { get; set; } = new LocalStorageSettings();
    public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();
    public S3Settings S3 { get; set; } = new S3Settings();
}

public class LocalStorageSettings
{
    public string LocalStoragePath { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}

public class CloudinarySettings
{
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}

public class S3Settings
{
    public string BucketName { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Region { get; set; } = null!;
}

public class FaceDetectionService
{
    public string BaseUrl { get; set; } = null!;
}
