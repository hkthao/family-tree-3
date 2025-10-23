namespace backend.Application.AI.VectorStore;

/// <summary>
/// Đại diện cho một truy vấn vào kho vector.
/// </summary>
public class VectorQuery
{
    /// <summary>
    /// Vector truy vấn.
    /// </summary>
    public double[] Vector { get; set; } = null!;
    /// <summary>
    /// Số lượng kết quả hàng đầu (Top K) cần trả về.
    /// </summary>
    public int TopK { get; set; }
    /// <summary>
    /// Bộ lọc tùy chọn để tinh chỉnh kết quả truy vấn.
    /// </summary>
    public Dictionary<string, string>? Filter { get; set; }
}
