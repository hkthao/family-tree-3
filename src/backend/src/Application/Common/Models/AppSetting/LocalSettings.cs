namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ AI cục bộ.
/// </summary>
public class LocalSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "LocalSettings";
    /// <summary>
    /// URL API của dịch vụ AI cục bộ.
    /// </summary>
    public string ApiUrl { get; set; } = null!;
    /// <summary>
    /// Tên mô hình AI cục bộ được sử dụng.
    /// </summary>
    public string Model { get; set; } = null!;
    /// <summary>
    /// Độ dài văn bản tối đa mà mô hình AI cục bộ có thể xử lý.
    /// </summary>
    public int MaxTextLength { get; set; }
}
