namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ lưu trữ cục bộ.
/// </summary>
public class LocalStorageSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "LocalStorageSettings";
    /// <summary>
    /// Đường dẫn lưu trữ cục bộ cho các tệp.
    /// </summary>
    public string LocalStoragePath { get; set; } = null!;
    /// <summary>
    /// URL cơ sở để truy cập các tệp được lưu trữ cục bộ.
    /// </summary>
    public string BaseUrl { get; set; } = null!;
}
