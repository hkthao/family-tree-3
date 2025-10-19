namespace backend.Application.Common.Models.AppSetting;

public class GeminiSettings
{
    public const string SectionName = "GeminiSettings";
    public string ApiKey { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string Region { get; set; } = null!;
}
