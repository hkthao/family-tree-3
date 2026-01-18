using backend.Application.AI.DTOs;
using backend.Application.Common.Models;
using backend.Application.Common.Models.LLMGateway; // New using

namespace backend.Application.AI.Commands.Chat.CallAiChatService;

/// <summary>
/// Lệnh để gọi dịch vụ AI Chat bên ngoài thông qua LLM Gateway Service.
/// </summary>
public record CallAiChatServiceCommand : IRequest<Result<ChatResponse>>
{
    /// <summary>
    /// Đối tượng LLMChatCompletionRequest chứa tất cả thông tin cần thiết.
    /// </summary>
    public LLMChatCompletionRequest LLMChatCompletionRequest { get; init; } = new LLMChatCompletionRequest();
}
