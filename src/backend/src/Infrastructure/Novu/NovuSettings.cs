
namespace backend.Infrastructure.Novu;

/// <summary>
/// Configuration settings for Novu.
/// </summary>
public class NovuSettings
{
    public const string SectionName = "Novu";
    public string ApiKey { get; set; } = string.Empty;
}
