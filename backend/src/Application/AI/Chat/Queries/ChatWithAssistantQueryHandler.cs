using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.AI.Chat.Queries;

public class ChatWithAssistantQueryHandler : IRequestHandler<ChatWithAssistantQuery, Result<ChatResponse>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly AIChatSettings _chatSettings;

    public ChatWithAssistantQueryHandler(IChatProviderFactory chatProviderFactory, AIChatSettings chatSettings)
    {
        _chatProviderFactory = chatProviderFactory;
        _chatSettings = chatSettings;
    }

    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var chatProvider = _chatProviderFactory.GetProvider(Enum.Parse<ChatAIProvider>(_chatSettings.Provider));
            var responseContent = await chatProvider.GenerateResponseAsync(request.UserMessage);

            var chatResponse = new ChatResponse
            {
                Response = responseContent,
                SessionId = request.SessionId,
                Model = _chatSettings.Provider.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            return Result<ChatResponse>.Success(chatResponse);
        }
        catch (Exception ex)
        {
            return Result<ChatResponse>.Failure($"Failed to generate chat response: {ex.Message}");
        }
    }
}
