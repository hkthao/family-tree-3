namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ Pinecone.
/// </summary>
public class PineconeSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "PineconeSettings";
    /// <summary>
    /// Khóa API để xác thực với Pinecone.
    /// </summary>
    public string ApiKey { get; set; } = null!;
    /// <summary>
    /// Host của dịch vụ Pinecone.
    /// </summary>
    public string Host { get; set; } = null!;
    /// <summary>
    /// Tên chỉ mục (index) trong Pinecone.
    /// </summary>
    public string IndexName { get; set; } = null!;
}
