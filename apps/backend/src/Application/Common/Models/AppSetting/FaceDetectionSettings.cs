namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ nhận diện khuôn mặt.
/// </summary>
public class FaceDetectionSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "FaceDetectionSettings";
    /// <summary>
    /// URL cơ sở của dịch vụ nhận diện khuôn mặt.
    /// </summary>
    public string BaseUrl { get; set; } = null!;
}
