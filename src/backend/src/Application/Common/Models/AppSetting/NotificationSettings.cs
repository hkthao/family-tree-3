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

    /// <summary>
    /// Nhà cung cấp thông báo đang hoạt động (ví dụ: "Novu", "Firebase").
    /// </summary>
    public string Provider { get; set; } = null!;
}