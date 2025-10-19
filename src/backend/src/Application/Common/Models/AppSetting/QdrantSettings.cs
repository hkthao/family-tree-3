namespace backend.Application.Common.Models.AppSetting;

public class QdrantSettings
{
    public const string SectionName = "QdrantSettings";
    public string Host { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
    public string VectorSize { get; set; } = null!;
}
