using backend.Application.Common.Models;

namespace backend.Application.AI.Chat;

/// <summary>
/// Lệnh để bắt đầu một cuộc trò chuyện với AI Assistant.
/// </summary>
public record ChatWithAssistantCommand : IRequest<Result<string>>
{
    /// <summary>
    /// ID phiên trò chuyện.
    /// </summary>
    public string SessionId { get; init; } = null!;

    /// <summary>
    /// Tin nhắn từ người dùng.
    /// </summary>
    public string Message { get; init; } = null!;
}
