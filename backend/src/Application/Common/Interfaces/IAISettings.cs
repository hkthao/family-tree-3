using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

public interface IAISettings
{
    AIProviderType Provider { get; set; }
    int MaxTokensPerRequest { get; set; }
    int DailyUsageLimit { get; set; }
}
