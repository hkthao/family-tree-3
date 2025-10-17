using backend.Application.Common.Models.AISettings;

namespace backend.Application.AI.VectorStore;

public class VectorStoreSettings
{
    public const string SectionName = "VectorStoreSettings";
    public string Provider { get; set; } = "";
    public int TopK { get; set; } = 3;
    public PineconeSettings Pinecone { get; set; } = null!;
    public QdrantSettings Qdrant { get; set; } = null!;
}
