namespace backend.Application.Common.Models.AppSetting;

/// <summary>
/// Đại diện cho các cài đặt cấu hình cho kho lưu trữ vector.
/// </summary>
public class VectorStoreSettings
{
    /// <summary>
    /// Tên của phần cấu hình trong tệp cài đặt.
    /// </summary>
    public const string SectionName = "VectorStoreSettings";
    /// <summary>
    /// Nhà cung cấp kho lưu trữ vector được sử dụng (ví dụ: Pinecone, Qdrant).
    /// </summary>
    public string Provider { get; set; } = null!;
    /// <summary>
    /// Số lượng kết quả hàng đầu (top K) cần truy vấn từ kho vector.
    /// </summary>
    public int TopK { get; set; }
    /// <summary>
    /// Cài đặt cụ thể cho dịch vụ Pinecone.
    /// </summary>
    public PineconeSettings Pinecone { get; set; } = new PineconeSettings();
    /// <summary>
    /// Cài đặt cụ thể cho dịch vụ Qdrant.
    /// </summary>
    public QdrantSettings Qdrant { get; set; } = new QdrantSettings();
}
