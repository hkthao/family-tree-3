namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ Gemini AI.
/// </summary>
public class GeminiSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "GeminiSettings";
    /// <summary>
    /// Khóa API để xác thực với Gemini.
    /// </summary>
    public string ApiKey { get; set; } = null!;
    /// <summary>
    /// Tên mô hình Gemini được sử dụng.
    /// </summary>
    public string Model { get; set; } = null!;
    /// <summary>
    /// Khu vực (region) triển khai dịch vụ Gemini.
    /// </summary>
    public string Region { get; set; } = null!;
}
