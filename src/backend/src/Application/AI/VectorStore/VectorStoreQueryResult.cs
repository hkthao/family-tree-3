namespace backend.Application.AI.VectorStore;

/// <summary>
/// Đại diện cho một kết quả truy vấn từ kho vector.
/// </summary>
public class VectorStoreQueryResult
{
    /// <summary>
    /// ID của tài liệu vector.
    /// </summary>
    public string Id { get; set; } = null!;
    /// <summary>
    /// Embedding của tài liệu vector.
    /// </summary>
    public List<double> Embedding { get; set; } = [];
    /// <summary>
    /// Các siêu dữ liệu (metadata) của tài liệu vector.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];
    /// <summary>
    /// Điểm số tương đồng của kết quả truy vấn.
    /// </summary>
    public double Score { get; set; }
    /// <summary>
    /// Nội dung của tài liệu vector.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
