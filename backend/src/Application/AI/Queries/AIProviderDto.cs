using backend.Domain.Enums;

namespace backend.Application.AI.Queries;

public class AIProviderDto
{
    public AIProviderType ProviderType { get; set; }
    public string Name { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public int DailyUsageLimit { get; set; }
    public int CurrentDailyUsage { get; set; }
    public int MaxTokensPerRequest { get; set; }
}
