using backend.Domain.Enums;

namespace backend.Application.AI.Common;

/// <summary>
/// Đại diện cho một yêu cầu gửi đến dịch vụ tạo nội dung AI.
/// </summary>
public class AIRequest
{
    /// <summary>
    /// Lời nhắc (prompt) của người dùng.
    /// </summary>
    public string UserPrompt { get; set; } = null!;
    /// <summary>
    /// Phong cách viết tiểu sử.
    /// </summary>
    public BiographyStyle Style { get; set; }
    /// <summary>
    /// Ngôn ngữ của nội dung được tạo. Mặc định là "Vietnamese".
    /// </summary>
    public string Language { get; set; } = "Vietnamese";
    /// <summary>
    /// Số lượng token tối đa cho phản hồi. Mặc định là 500.
    /// </summary>
    public int MaxTokens { get; set; } = 500;
    /// <summary>
    /// Cho biết nội dung có được tạo từ cơ sở dữ liệu hay không. Mặc định là false.
    /// </summary>
    public bool GeneratedFromDB { get; set; } = false;
    /// <summary>
    /// ID của thành viên liên quan.
    /// </summary>
    public Guid MemberId { get; set; }
}
