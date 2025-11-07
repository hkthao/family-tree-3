using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IN8nService
{
    Task<Result<string>> CallChatWebhookAsync(string message, List<ChatMessage> history, CancellationToken cancellationToken);
    Task<Result<string>> CallEmbeddingWebhookAsync(EmbeddingWebhookDto dto, CancellationToken cancellationToken);
}
