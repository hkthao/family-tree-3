namespace backend.Application.Common.Models.AppSetting;

public class OpenAISettings
{
    public const string SectionName = "OpenAISettings";
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Organization { get; set; } = null!;
    public int MaxTextLength { get; set; } = 8191; // Default to OpenAI's max token limit for embeddings
}
