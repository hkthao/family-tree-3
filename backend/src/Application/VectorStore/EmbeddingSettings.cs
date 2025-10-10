using backend.Application.Common.Models.AISettings;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Application.VectorStore;

public class EmbeddingSettings
{
    public const string SectionName = "EmbeddingSettings";

    public AIProviderType Provider { get; set; }
    public VectorStoreProviderType VectorStoreProvider { get; set; }
    public int MaxTokensPerRequest { get; set; }
    public int DailyUsageLimit { get; set; }

    public Dictionary<string, IAIProviderSettings> Providers { get; set; } = new Dictionary<string, IAIProviderSettings>();

    public PineconeSettings Pinecone { get; set; } = null!;
}
