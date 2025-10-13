using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Application.AI.VectorStore;
using backend.Infrastructure.AI.Embeddings;

namespace backend.Application.AI.Chat.Queries;

public class ChatWithAssistantQueryHandler : IRequestHandler<ChatWithAssistantQuery, Result<ChatResponse>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly AIChatSettings _chatSettings;
    private readonly EmbeddingSettings _embeddingSettings;
    private readonly VectorStoreSettings _vectorStoreSettings;

    public ChatWithAssistantQueryHandler(
        IChatProviderFactory chatProviderFactory,
        IEmbeddingProviderFactory embeddingProviderFactory,
        IVectorStoreFactory vectorStoreFactory,
        AIChatSettings chatSettings,
        EmbeddingSettings embeddingSettings,
        VectorStoreSettings vectorStoreSettings)
    {
        _chatProviderFactory = chatProviderFactory;
        _embeddingProviderFactory = embeddingProviderFactory;
        _vectorStoreFactory = vectorStoreFactory;
        _chatSettings = chatSettings;
        _embeddingSettings = embeddingSettings;
        _vectorStoreSettings = vectorStoreSettings;
    }

    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Generate embedding for the user's prompt
            var embeddingProvider = _embeddingProviderFactory.GetProvider(Enum.Parse<EmbeddingAIProvider>(_embeddingSettings.Provider));
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(request.UserMessage, cancellationToken);

            if (embeddingResult.IsFailure)
            {
                return Result<ChatResponse>.Failure($"Failed to generate embedding: {embeddingResult.Error}");
            }

            var userEmbedding = embeddingResult.Value;

            // 2. Search in vector store
            var vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));
            var searchResults = await vectorStore.QueryAsync(userEmbedding, _vectorStoreSettings.TopK, new Dictionary<string, string>(), cancellationToken);

            // 3. Construct augmented prompt
            var context = string.Join("\n\n", searchResults.Select(c => c.Content));
            var augmentedPrompt = $"Context: {context}\n\nUser: {request.UserMessage}";

            // 4. Get chat response
            var chatProvider = _chatProviderFactory.GetProvider(Enum.Parse<ChatAIProvider>(_chatSettings.Provider));
            var responseContent = await chatProvider.GenerateResponseAsync(augmentedPrompt);

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
