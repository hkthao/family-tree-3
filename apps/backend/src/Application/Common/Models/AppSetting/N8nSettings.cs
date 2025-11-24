namespace backend.Application.Common.Models.AppSetting;

public class N8nSettings
{
    public const string SectionName = "N8n"; // Keep "N8n" as the SectionName
    public string BaseUrl { get; set; } = string.Empty;
    public string PhotoAnalysisWebhook { get; set; } = string.Empty;
    public string StoryGenerationWebhook { get; set; } = string.Empty;
    public string ChatWebhookUrl { get; set; } = string.Empty; // Added
    public string EmbeddingWebhookUrl { get; set; } = string.Empty; // Added
    public string CollectionName { get; set; } = string.Empty; // Added
    public string JwtSecret { get; set; } = string.Empty; // Added
}