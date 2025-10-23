namespace backend.Application.AI.Chat;

/// <summary>
/// Đại diện cho phản hồi từ dịch vụ trò chuyện AI.
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// Nội dung phản hồi từ AI.
    /// </summary>
    public string Response { get; set; } = string.Empty;
    /// <summary>
    /// Ngữ cảnh (context) được sử dụng để tạo phản hồi.
    /// </summary>
    public List<string> Context { get; set; } = [];
    /// <summary>
    /// ID phiên trò chuyện.
    /// </summary>
    public string? SessionId { get; set; }
    /// <summary>
    /// Mô hình AI được sử dụng để tạo phản hồi.
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// Thời gian tạo phản hồi.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
