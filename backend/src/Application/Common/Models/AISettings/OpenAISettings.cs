using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Models.AISettings;

public class OpenAISettings : IAIProviderSettings
{
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Organization { get; set; } = null!;
}
