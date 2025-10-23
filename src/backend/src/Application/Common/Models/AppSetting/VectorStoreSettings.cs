namespace backend.Application.Common.Models.AppSetting;

public class VectorStoreSettings
{
    public const string SectionName = "VectorStoreSettings";
    public string Provider { get; set; } = null!;
    public int TopK { get; set; }
    public PineconeSettings Pinecone { get; set; } = new PineconeSettings();
    public QdrantSettings Qdrant { get; set; } = new QdrantSettings();
}
