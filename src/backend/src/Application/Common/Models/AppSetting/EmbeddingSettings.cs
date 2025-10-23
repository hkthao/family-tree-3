namespace backend.Application.Common.Models.AppSetting;

public class EmbeddingSettings
{
    public const string SectionName = "EmbeddingSettings";
    public string Provider { get; set; } = null!;
    public OpenAISettings OpenAI { get; set; } = new OpenAISettings();
    public CohereSettings Cohere { get; set; } = new CohereSettings();
    public LocalSettings Local { get; set; } = new LocalSettings();
}
