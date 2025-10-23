namespace backend.Application.Common.Models.AppSetting;

public class PineconeSettings
{
    public const string SectionName = "PineconeSettings";
    public string ApiKey { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string IndexName { get; set; } = null!;
}
