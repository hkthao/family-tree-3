namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ lưu trữ Amazon S3.
/// </summary>
public class S3Settings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "S3Settings";
    /// <summary>
    /// Tên bucket S3.
    /// </summary>
    public string BucketName { get; set; } = null!;
    /// <summary>
    /// Khóa truy cập (Access Key) để xác thực với S3.
    /// </summary>
    public string AccessKey { get; set; } = null!;
    /// <summary>
    /// Khóa bí mật (Secret Key) để xác thực với S3.
    /// </summary>
    public string SecretKey { get; set; } = null!;
    /// <summary>
    /// Khu vực (Region) của bucket S3.
    /// </summary>
    public string Region { get; set; } = null!;
}
