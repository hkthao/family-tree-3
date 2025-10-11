using backend.Application.Common.Models.AISettings;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Application.AI.Embeddings;

public class EmbeddingSettings
{
    public const string SectionName = "EmbeddingSettings";

    public AIProviderType Provider { get; set; }
    public int MaxTokensPerRequest { get; set; }
    public int DailyUsageLimit { get; set; }

    public Dictionary<string, IAIProviderSettings> Providers { get; set; } = [];
}
