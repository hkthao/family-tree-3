using backend.Domain.Enums;
using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Models.AISettings
{
    public class AIProviderConfig
    {
        public AIProviderType Provider { get; set; }
        public int MaxTokensPerRequest { get; set; }
        public int DailyUsageLimit { get; set; }
        public Dictionary<string, IAIProviderSettings> Providers { get; set; } = [];
    }
}
