using backend.Application.AI.DTOs;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using MediatR; // Ensure MediatR is imported

namespace backend.Application.AI.Chat;

/// <summary>
/// Handler cho lệnh <see cref="ChatWithAssistantCommand"/>.
/// </summary>
public class ChatWithAssistantCommandHandler : IRequestHandler<ChatWithAssistantCommand, Result<ChatResponse>>
{
    private readonly IChatAiService _chatAiService;

    public ChatWithAssistantCommandHandler(IChatAiService chatAiService)
    {
        _chatAiService = chatAiService;
    }

    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantCommand request, CancellationToken cancellationToken)
    {
        var chatRequest = new CallChatWebhookRequest
        {
            SessionId = request.SessionId,
            ChatInput = request.ChatInput, // Corrected to ChatInput, keeping request.Message as source for now
            Metadata = request.Metadata // Pass the metadata from the request
        };
        return await _chatAiService.CallChatWebhookAsync(chatRequest, cancellationToken);
    }
}
