using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.AI.Chat;

/// <summary>
/// Handler cho lệnh <see cref="ChatWithAssistantCommand"/>.
/// </summary>
public class ChatWithAssistantCommandHandler : IRequestHandler<ChatWithAssistantCommand, Result<string>>
{
    private readonly IN8nService _n8nService;

    public ChatWithAssistantCommandHandler(IN8nService n8nService)
    {
        _n8nService = n8nService;
    }

    public async Task<Result<string>> Handle(ChatWithAssistantCommand request, CancellationToken cancellationToken)
    {
        return await _n8nService.CallChatWebhookAsync(request.Message, request.History, cancellationToken);
    }
}
