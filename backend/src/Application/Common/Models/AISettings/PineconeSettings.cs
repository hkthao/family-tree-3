namespace backend.Application.Common.Models.AISettings;

public class PineconeSettings
{
    public string ApiKey { get; set; } = null!;
    public string Environment { get; set; } = null!;
    public string IndexName { get; set; } = null!;
}
