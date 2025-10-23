namespace backend.Application.Common.Models.AppSetting;

public class S3Settings
{
    public const string SectionName = "S3Settings";
    public string BucketName { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Region { get; set; } = null!;
}
