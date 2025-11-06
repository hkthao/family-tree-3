using backend.Application.Common.Models;

namespace backend.Application.AI.Chat;

/// <summary>
/// Lệnh để bắt đầu một cuộc trò chuyện với AI Assistant.
/// </summary>
public record ChatWithAssistantCommand : IRequest<Result<string>>
{
    /// <summary>
    /// Tin nhắn từ người dùng.
    /// </summary>
    public string Message { get; init; } = null!;

    /// <summary>
    /// Lịch sử của cuộc trò chuyện (tùy chọn).
    /// </summary>
    public List<ChatMessage> History { get; init; } = [];
}
