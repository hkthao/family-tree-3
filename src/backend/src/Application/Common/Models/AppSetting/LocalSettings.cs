namespace backend.Application.Common.Models.AppSetting;

public class LocalSettings
{
    public const string SectionName = "LocalSettings";
    public string ApiUrl { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int MaxTextLength { get; set; }
}
