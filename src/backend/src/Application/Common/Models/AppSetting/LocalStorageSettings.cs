namespace backend.Application.Common.Models.AppSetting;

public class LocalStorageSettings
{
    public const string SectionName = "LocalStorageSettings";
    public string LocalStoragePath { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}
