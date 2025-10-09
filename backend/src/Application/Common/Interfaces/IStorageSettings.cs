namespace backend.Application.Common.Interfaces;

public interface IStorageSettings
{
    string Provider { get; }
    long MaxFileSizeMB { get; }
    string LocalStoragePath { get; }
    string BaseUrl { get; }
    ICloudinarySettings Cloudinary { get; }
    IS3Settings S3 { get; }
}

public interface ICloudinarySettings
{
    string CloudName { get; }
    string ApiKey { get; }
    string ApiSecret { get; }
}

public interface IS3Settings
{
    string BucketName { get; }
    string AccessKey { get; }
    string SecretKey { get; }
    string Region { get; }
}
