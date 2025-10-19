namespace backend.Application.Common.Models.AppSetting;

public class StorageSettings
{
    public const string SectionName = "StorageSettings";
    public string Provider { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public int MaxFileSizeMB { get; set; } = 5; // Default to 5MB
    public LocalStorageSettings Local { get; set; } = new LocalStorageSettings();
    public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();
    public S3Settings S3 { get; set; } = new S3Settings();
}
