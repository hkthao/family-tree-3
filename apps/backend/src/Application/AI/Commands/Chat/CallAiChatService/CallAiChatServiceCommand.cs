using backend.Application.AI.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.AI.Commands.Chat.CallAiChatService;

/// <summary>
/// Lệnh để gọi dịch vụ AI Chat bên ngoài.
/// </summary>
public record CallAiChatServiceCommand : IRequest<Result<ChatResponse>>
{
    /// <summary>
    /// Đối tượng ChatRequest chứa tất cả thông tin cần thiết.
    /// </summary>
    public ChatRequest ChatRequest { get; init; } = new ChatRequest();
}
