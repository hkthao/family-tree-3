namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ Cloudinary.
/// </summary>
public class CloudinarySettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "CloudinarySettings";
    /// <summary>
    /// Tên đám mây (Cloud Name) của tài khoản Cloudinary.
    /// </summary>
    public string CloudName { get; set; } = null!;
    /// <summary>
    /// Khóa API để xác thực với Cloudinary.
    /// </summary>
    public string ApiKey { get; set; } = null!;
    /// <summary>
    /// Khóa bí mật API để xác thực với Cloudinary.
    /// </summary>
    public string ApiSecret { get; set; } = null!;
}
