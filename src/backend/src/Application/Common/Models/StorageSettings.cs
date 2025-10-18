namespace backend.Application.Common.Models;

public class StorageSettings
{
    public const string SectionName = "StorageSettings";
    public long MaxFileSizeMB { get; set; } = 5;
    public string Provider { get; set; } = "Local";
    public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();
    public S3Settings S3 { get; set; } = new S3Settings();
    public LocalStorageSettings Local { get; set; } = new LocalStorageSettings();
}

public class LocalStorageSettings
{
    public string LocalStoragePath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = null!;
}

public class CloudinarySettings
{
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}

public class S3Settings
{
    public string BucketName { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Region { get; set; } = null!;
}
