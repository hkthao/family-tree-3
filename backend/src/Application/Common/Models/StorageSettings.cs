namespace backend.Application.Common.Models;

public class StorageSettings
{
    public string Provider { get; set; } = "Local"; // Default to Local
    public long MaxFileSizeMB { get; set; } = 5; // Default max file size to 5 MB
    public string LocalStoragePath { get; set; } = "wwwroot/uploads";
    public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();
    public S3Settings S3 { get; set; } = new S3Settings();
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
