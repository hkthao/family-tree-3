using backend.Application.Common.Interfaces;

namespace backend.Application.AI.Chat.Queries;

public class ChatWithAssistantQueryHandler : IRequestHandler<ChatWithAssistantQuery, ChatResponse>
{
    private readonly IChatService _chatService;

    public ChatWithAssistantQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ChatResponse> Handle(ChatWithAssistantQuery request, CancellationToken cancellationToken)
    {
        // The ChatService already handles context retrieval and LLM interaction
        return await _chatService.SendMessageAsync(request.UserMessage, request.SessionId);
    }
}
