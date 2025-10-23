namespace backend.Application.AI.VectorStore;

/// <summary>
/// Đại diện cho một tài liệu vector được lưu trữ trong Vector Store.
/// </summary>
public class VectorDocument
{
    /// <summary>
    /// ID duy nhất của tài liệu.
    /// </summary>
    public string Id { get; set; } = null!;
    /// <summary>
    /// Nội dung văn bản gốc của tài liệu.
    /// </summary>
    public string Content { get; set; } = null!;
    /// <summary>
    /// Biểu diễn vector (embedding) của tài liệu.
    /// </summary>
    public double[] Vector { get; set; } = null!;
    /// <summary>
    /// Các siêu dữ liệu (metadata) bổ sung liên quan đến tài liệu.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
