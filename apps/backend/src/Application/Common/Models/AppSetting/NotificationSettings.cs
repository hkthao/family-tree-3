namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình tổng thể cho dịch vụ thông báo.
/// </summary>
public class NotificationSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "NotificationSettings";
    public string? BaseUrl { get; set; }
}
