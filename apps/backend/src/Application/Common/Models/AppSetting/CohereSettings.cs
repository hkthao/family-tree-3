namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ Cohere.
/// </summary>
public class CohereSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "CohereSettings";
    /// <summary>
    /// Khóa API để xác thực với Cohere.
    /// </summary>
    public string ApiKey { get; set; } = null!;
    /// <summary>
    /// Tên mô hình Cohere được sử dụng.
    /// </summary>
    public string Model { get; set; } = null!;
    /// <summary>
    /// Độ dài văn bản tối đa mà mô hình Cohere có thể xử lý.
    /// </summary>
    public int MaxTextLength { get; set; }
}
