namespace backend.Application.Common.Models.AppSetting;

public class LLMGatewaySettings
{
    public const string SectionName = "LLMGatewayService"; // Matches configuration section
    public string LlmModel { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}
