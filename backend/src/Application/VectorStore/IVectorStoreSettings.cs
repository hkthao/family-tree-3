namespace backend.Application.VectorStore;

public interface IVectorStoreSettings
{
    string Provider { get; }
    PineconeSettings Pinecone { get; }
}

public class PineconeSettings
{
    public string ApiKey { get; set; } = null!;
    public string Environment { get; set; } = null!;
    public string IndexName { get; set; } = null!;
}
