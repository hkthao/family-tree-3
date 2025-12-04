using backend.Application.AI.DTOs;
using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IChatAiService
{
    Task<Result<ChatResponse>> CallChatWebhookAsync(CallChatWebhookRequest request, CancellationToken cancellationToken);
}
