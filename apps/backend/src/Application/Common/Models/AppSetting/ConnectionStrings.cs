namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các chuỗi kết nối cơ sở dữ liệu của ứng dụng.
/// </summary>
public class ConnectionStrings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "ConnectionStrings";
    /// <summary>
    /// Chuỗi kết nối mặc định đến cơ sở dữ liệu.
    /// </summary>
    public string DefaultConnection { get; set; } = null!;
}
