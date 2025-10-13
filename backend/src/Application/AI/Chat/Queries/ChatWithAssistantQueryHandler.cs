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
            string finalPromptContent;

            // Define the system prompt
            var systemPrompt = new ChatMessage
            {
                Role = "system",
                Content = @"Bạn là trợ lý AI của hệ thống FamilyTree.
                            Chỉ trả lời các câu hỏi liên quan tới chức năng phần mềm, hướng dẫn sử dụng, cấu hình, và troubleshooting.
                            Các câu hỏi ngoài phạm vi phần mềm (chào hỏi, small talk, câu cá nhân, phi kỹ thuật) thì trả về '(Không có dữ liệu liên quan)'.
                            Trả lời bằng tiếng Việt, ngắn gọn, dễ hiểu."
            };


            // Proceed with RAG flow
            // 1. Generate embedding for the user's prompt
            var embeddingProvider = _embeddingProviderFactory.GetProvider(Enum.Parse<EmbeddingAIProvider>(_embeddingSettings.Provider));
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(request.UserMessage, cancellationToken);

            if (!embeddingResult.IsSuccess)
                return Result<ChatResponse>.Failure($"Failed to generate embedding: {embeddingResult.Error}");

            var userEmbedding = embeddingResult.Value!;

            // 2. Search in vector store
            var vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(_vectorStoreSettings.Provider));
            var silimarityResults = await vectorStore.QueryAsync(userEmbedding, _vectorStoreSettings.TopK, [], cancellationToken);
            var searchResults = silimarityResults.Where(s=>s.Score > 0.75).ToList();

            // 3. Check if relevant context exists
            if (searchResults == null || searchResults.Count == 0)
            {
                _logger.LogInformation("No relevant context found for user message: {UserMessage}. Using fallback prompt.", request.UserMessage);
                finalPromptContent = "Tôi không tìm thấy thông tin liên quan trong cơ sở dữ liệu. Bạn có câu hỏi nào khác về phần mềm không?"; // Fallback for no context
            }
            else
            {
                // 4. Construct augmented prompt
                var context = string.Join("\n\n", searchResults.Select(c => c.Content));
                finalPromptContent = $"Context: {context}\n\nUser: {request.UserMessage}";
            }

            // Construct messages list for the chat provider
            var messages = new List<ChatMessage>
            {
                systemPrompt,
                new() { Role = "user", Content = finalPromptContent }
            };

            // 5. Get chat response using the determined prompt
            var chatProvider = _chatProviderFactory.GetProvider(Enum.Parse<ChatAIProvider>(_chatSettings.Provider));
            var responseContent = await chatProvider.GenerateResponseAsync(messages);

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
