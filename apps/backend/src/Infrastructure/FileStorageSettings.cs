namespace backend.Infrastructure;

public class FileStorageSettings
{
    public const string SectionName = "FileStorageSettings";

    public string Provider { get; set; } = "Local"; // Default provider
    public LocalFileStorageSettings Local { get; set; } = new LocalFileStorageSettings();
}

public class LocalFileStorageSettings
{
    public string LocalStoragePath { get; set; } = "./uploads";
    public string BaseUrl { get; set; } = "http://localhost:5000/files";
}

public class CloudflareR2Settings
{
    public const string SectionName = "CloudflareR2Settings";

    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty; // e.g., https://<ACCOUNT_ID>.r2.cloudflarestorage.com
}
