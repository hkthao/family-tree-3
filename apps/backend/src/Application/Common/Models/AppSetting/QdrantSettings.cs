namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho dịch vụ Qdrant.
/// </summary>
public class QdrantSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "QdrantSettings";
    /// <summary>
    /// Host của dịch vụ Qdrant.
    /// </summary>
    public string Host { get; set; } = null!;
    /// <summary>
    /// Khóa API để xác thực với Qdrant.
    /// </summary>
    public string ApiKey { get; set; } = null!;
    /// <summary>
    /// Tên collection trong Qdrant.
    /// </summary>
    public string CollectionName { get; set; } = null!;
    /// <summary>
    /// Kích thước vector được sử dụng trong Qdrant.
    /// </summary>
    public string VectorSize { get; set; } = null!;
}
