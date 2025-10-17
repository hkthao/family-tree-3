namespace backend.Application.Common.Models.AISettings;

public class QdrantSettings
{
    public string Host { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
    public int VectorSize { get; set; } = 1024; // Default for OpenAI embeddings
}
