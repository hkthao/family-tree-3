using backend.Application.Common.Models.AISettings;
using backend.Domain.Enums;

namespace backend.Application.AI.VectorStore;

public class VectorStoreSettings
{
    public const string SectionName = "VectorStoreSettings";

    public VectorStoreProviderType VectorStoreProvider { get; set; }
    public PineconeSettings Pinecone { get; set; } = null!;
}
