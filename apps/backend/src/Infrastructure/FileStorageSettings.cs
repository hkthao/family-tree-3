namespace backend.Infrastructure;

public class FileStorageSettings
{
    public const string SectionName = "FileStorageSettings";

    public string Provider { get; set; } = "Imgur"; // Default provider
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
