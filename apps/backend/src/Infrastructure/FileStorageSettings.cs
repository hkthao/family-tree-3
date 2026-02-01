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
