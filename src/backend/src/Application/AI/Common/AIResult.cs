using backend.Domain.Enums;

namespace backend.Application.AI.Common;

/// <summary>
/// Đại diện cho kết quả từ dịch vụ tạo nội dung AI.
/// </summary>
public class AIResult
{
    /// <summary>
    /// Nội dung được tạo bởi AI.
    /// </summary>
    public string Content { get; set; } = null!;
    /// <summary>
    /// Số lượng token đã sử dụng để tạo nội dung.
    /// </summary>
    public int TokensUsed { get; set; }
    /// <summary>
    /// Nhà cung cấp AI đã được sử dụng.
    /// </summary>
    public AIProviderType Provider { get; set; }
    /// <summary>
    /// Thời gian nội dung được tạo.
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    /// <summary>
    /// Thông báo lỗi nếu có.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// Cho biết liệu thao tác có thành công hay không.
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
}
