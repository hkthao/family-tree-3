using backend.Application.VectorStore;

namespace backend.Infrastructure.VectorStore;

public class VectorStoreSettings : IVectorStoreSettings
{
    public string Provider { get; set; } = null!;
    public PineconeSettings Pinecone { get; set; } = null!;
}

