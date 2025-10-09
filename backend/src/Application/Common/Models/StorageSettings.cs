using backend.Application.Common.Interfaces;

namespace backend.Application.Common.Models;

public class StorageSettings : IStorageSettings
{
    public string Provider { get; set; } = "Local"; // Default to Local
    public long MaxFileSizeMB { get; set; } = 5; // Default max file size to 5 MB
    public string LocalStoragePath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = null!; // Base URL for local storage, e.g., http://localhost:5000
    public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();
    public S3Settings S3 { get; set; } = new S3Settings();

    ICloudinarySettings IStorageSettings.Cloudinary => Cloudinary;
    IS3Settings IStorageSettings.S3 => S3;
}

public class CloudinarySettings : ICloudinarySettings
{
    public string CloudName { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string ApiSecret { get; set; } = null!;
}

public class S3Settings : IS3Settings
{
    public string BucketName { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Region { get; set; } = null!;
}
