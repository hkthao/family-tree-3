namespace backend.Application.Common.Models.AppSetting;

public class N8nSettings
{
    public const string SectionName = "N8nSettings";
    public string BaseUrl { get; set; } = string.Empty;
    public ChatSettings Chat { get; set; } = new ChatSettings();
    public UploadSettings Upload { get; set; } = new UploadSettings();
    public FaceSettings Face { get; set; } = new FaceSettings();
    public EmbeddingsSettings Embeddings { get; set; } = new EmbeddingsSettings();
    public string JwtSecret { get; set; } = string.Empty;
}

public class ChatSettings
{
     public string ChatWebhookUrl { get; set; } = string.Empty;
    public string GenerateWebhookUrl { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
}

public class UploadSettings
{
    public string WebHookUrl { get; set; } = string.Empty;
    public string Cloud { get; set; } = "cloudinary";
}

public class FaceSettings
{
    public string CollectionName { get; set; } = string.Empty;
    public string UpsertWebhookUrl { get; set; } = string.Empty;
    public string SearchWebhookUrl { get; set; } = string.Empty;
    public string DeleteWebhookUrl { get; set; } = string.Empty;
    // Potentially other face-related settings in the future
}

public class EmbeddingsSettings // NEW
{
    public string WebHookUrl { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
}
