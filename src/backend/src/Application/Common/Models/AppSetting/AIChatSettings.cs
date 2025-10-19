namespace backend.Application.Common.Models.AppSetting;

public class AIChatSettings
{
    public const string SectionName = "AIChatSettings";
    public string Provider { get; set; } = null!;
    public int ScoreThreshold { get; set; }
    public GeminiSettings Gemini { get; set; } = new GeminiSettings();
    public OpenAISettings OpenAI { get; set; } = new OpenAISettings();
    public LocalSettings Local { get; set; } = new LocalSettings();
}
