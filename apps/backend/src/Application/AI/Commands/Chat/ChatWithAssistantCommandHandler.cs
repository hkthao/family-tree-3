using backend.Application.AI.DTOs;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.AI.Chat;

/// <summary>
/// Handler cho lệnh <see cref="ChatWithAssistantCommand"/>.
/// </summary>
public class ChatWithAssistantCommandHandler : IRequestHandler<ChatWithAssistantCommand, Result<ChatResponse>>
{
    private readonly IAiChatService _chatAiService;

    public ChatWithAssistantCommandHandler(IAiChatService chatAiService)
    {
        _chatAiService = chatAiService;
    }

    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantCommand request, CancellationToken cancellationToken)
    {
        var chatRequest = new ChatRequest
        {
            SessionId = request.SessionId,
            ChatInput = request.ChatInput, // Corrected to ChatInput, keeping request.Message as source for now
            Metadata = request.Metadata // Pass the metadata from the request
        };
        return await _chatAiService.CallChatWebhookAsync(chatRequest, cancellationToken);
    }
}
