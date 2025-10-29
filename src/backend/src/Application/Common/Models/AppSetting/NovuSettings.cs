
namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Configuration settings for Novu.
/// </summary>
public class NovuSettings
{
    public const string SectionName = "NovuSettings";
    public string ApiKey { get; set; } = string.Empty;
}
