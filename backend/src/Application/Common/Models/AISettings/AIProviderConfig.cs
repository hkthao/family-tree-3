using backend.Domain.Enums;

namespace backend.Application.Common.Models.AISettings
{
    public class AIProviderConfig
    {
        public AIProviderType Provider { get; set; }
        public int MaxTokensPerRequest { get; set; }
        public int DailyUsageLimit { get; set; }
        public Dictionary<string, object> Providers { get; set; } = [];
    }
}
