namespace backend.Application.Common.Models.AppSetting;

public class N8nSettings
{
    public const string SectionName = "N8nSettings";
    public string BaseUrl { get; set; } = string.Empty;
    public string ChatWebhookUrl { get; set; } = string.Empty;
    public string EmbeddingWebhookUrl { get; set; } = string.Empty;
    public string ImageUploadWebhookUrl { get; set; } = string.Empty; // NEW PROPERTY
    public string FaceVectorWebhookUrl { get; set; } = string.Empty; // NEW PROPERTY
    public FaceSettings Face { get; set; } = new FaceSettings(); // New nested setting
    public string JwtSecret { get; set; } = string.Empty;
}

public class FaceSettings
{
    public string CollectionName { get; set; } = string.Empty;
    // Potentially other face-related settings in the future
}
