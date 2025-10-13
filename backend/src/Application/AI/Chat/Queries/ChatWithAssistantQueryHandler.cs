using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Enums;
using backend.Application.AI.VectorStore;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Chat.Queries;

public class ChatWithAssistantQueryHandler : IRequestHandler<ChatWithAssistantQuery, Result<ChatResponse>>
{
    private readonly IChatProviderFactory _chatProviderFactory;
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly AIChatSettings _chatSettings;
    private readonly EmbeddingSettings _embeddingSettings;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly ILogger<ChatWithAssistantQueryHandler> _logger;

    public ChatWithAssistantQueryHandler(
        IChatProviderFactory chatProviderFactory,
        IEmbeddingProviderFactory embeddingProviderFactory,
        IVectorStoreFactory vectorStoreFactory,
        AIChatSettings chatSettings,
        EmbeddingSettings embeddingSettings,
        VectorStoreSettings vectorStoreSettings,
        ILogger<ChatWithAssistantQueryHandler> logger)
    {
        _chatProviderFactory = chatProviderFactory;
        _embeddingProviderFactory = embeddingProviderFactory;
        _vectorStoreFactory = vectorStoreFactory;
        _chatSettings = chatSettings;
        _embeddingSettings = embeddingSettings;
        _vectorStoreSettings = vectorStoreSettings;
        _logger = logger;
    }

    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantQuery request, CancellationToken cancellationToken)
    {
        try
        {
            string finalPrompt;

            // Proceed with RAG flow
            // 1. Generate embedding for the user's prompt
            var embeddingProvider = _embeddingProviderFactory.GetProvider(Enum.Parse<EmbeddingAIProvider>(_embeddingSettings.Provider));
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(request.UserMessage, cancellationToken);

            if (!embeddingResult.IsSuccess || embeddingResult.Value == null)
                return Result<ChatResponse>.Failure($"Failed to generate embedding: {embeddingResult.Error}");

            var userEmbedding = embeddingResult.Value!;

            // 2. Search in vector store
            var vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));
            var searchResults = await vectorStore.QueryAsync(userEmbedding, _vectorStoreSettings.TopK, [], cancellationToken);

            // 3. Check if relevant context exists
            if (searchResults == null || searchResults.Count == 0)
            {
                _logger.LogInformation("No relevant context found for user message: {UserMessage}. Using fallback prompt.", request.UserMessage);
                finalPrompt = "Tôi không tìm thấy thông tin liên quan trong cơ sở dữ liệu. Bạn có câu hỏi nào khác về phần mềm không?"; // Fallback for no context
            }
            else
            {
                // 4. Construct augmented prompt
                var context = string.Join("\n\n", searchResults.Select(c => c.Content));
                finalPrompt = $"Context: {context}\n\nUser: {request.UserMessage}";
            }

            _logger.LogInformation("Final prompt: {finalPrompt}", finalPrompt);

            // 5. Get chat response using the determined prompt
            var chatProvider = _chatProviderFactory.GetProvider(Enum.Parse<ChatAIProvider>(_chatSettings.Provider));
            var responseContent = await chatProvider.GenerateResponseAsync(finalPrompt);

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
