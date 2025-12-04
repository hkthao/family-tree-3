using backend.Application.AI.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IAiChatService
{
    Task<Result<ChatResponse>> CallChatWebhookAsync(ChatRequest request, CancellationToken cancellationToken);
}
