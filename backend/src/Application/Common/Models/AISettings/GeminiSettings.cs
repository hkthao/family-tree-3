using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Models.AISettings
{
    public class GeminiSettings : IAIProviderSettings
    {
        public string ApiKey { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string Region { get; set; } = null!;
    }
}
