namespace backend.Application.Common.Models;

/// <summary>
/// Đại diện cho một tin nhắn trong cuộc trò chuyện AI.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Vai trò của người gửi tin nhắn (ví dụ: "user", "assistant").
    /// </summary>
    public string Role { get; set; } = null!;
    /// <summary>
    /// Nội dung của tin nhắn.
    /// </summary>
    public string Content { get; set; } = null!;
}
