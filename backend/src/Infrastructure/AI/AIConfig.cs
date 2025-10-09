using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Infrastructure.AI;

public class AIConfig : IAISettings
{
    public AIProviderType Provider { get; set; }
    public GeminiConfig? Gemini { get; set; }
    public OpenAIConfig? OpenAI { get; set; }
    public LocalAIConfig? LocalAI { get; set; }
    public int MaxTokensPerRequest { get; set; } = 500;
    public int DailyUsageLimit { get; set; } = 10000;
}

public class GeminiConfig
{
    public string? ApiKey { get; set; }
}

public class OpenAIConfig
{
    public string? ApiKey { get; set; }
    public string? Organization { get; set; }
}

public class LocalAIConfig
{
    public string? Endpoint { get; set; }
}
