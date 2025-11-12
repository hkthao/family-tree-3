namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ OpenAI.
/// </summary>
public class OpenAISettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "OpenAISettings";
    /// <summary>
    /// Khóa API để xác thực với OpenAI.
    /// </summary>
    public string ApiKey { get; set; } = null!;
    /// <summary>
    /// Tên mô hình OpenAI được sử dụng.
    /// </summary>
    public string Model { get; set; } = null!;
    /// <summary>
    /// ID tổ chức OpenAI.
    /// </summary>
    public string Organization { get; set; } = null!;
    /// <summary>
    /// Độ dài văn bản tối đa mà mô hình OpenAI có thể xử lý.
    /// </summary>
    public int MaxTextLength { get; set; } = 8191; // Default to OpenAI's max token limit for embeddings
}
