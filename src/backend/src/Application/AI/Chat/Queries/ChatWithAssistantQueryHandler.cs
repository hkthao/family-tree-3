using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Chat.Queries;

/// <summary>
/// Xử lý truy vấn ChatWithAssistantQuery để tạo phản hồi trò chuyện từ trợ lý AI.
/// </summary>
public class ChatWithAssistantQueryHandler(
    IChatProviderFactory chatProviderFactory,
    IEmbeddingProviderFactory embeddingProviderFactory,
    IVectorStoreFactory vectorStoreFactory,
    IConfigProvider configProvider,
    ILogger<ChatWithAssistantQueryHandler> logger) : IRequestHandler<ChatWithAssistantQuery, Result<ChatResponse>>
{
    /// <summary>
    /// Nhà máy tạo nhà cung cấp trò chuyện AI.
    /// </summary>
    private readonly IChatProviderFactory _chatProviderFactory = chatProviderFactory;
    /// <summary>
    /// Nhà máy tạo nhà cung cấp nhúng (embedding) AI.
    /// </summary>
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory = embeddingProviderFactory;
    /// <summary>
    /// Nhà máy tạo kho lưu trữ vector.
    /// </summary>
    private readonly IVectorStoreFactory _vectorStoreFactory = vectorStoreFactory;
    /// <summary>
    /// Đối tượng ghi log.
    /// </summary>
    private readonly ILogger<ChatWithAssistantQueryHandler> _logger = logger;
    /// <summary>
    /// Nhà cung cấp cấu hình ứng dụng.
    /// </summary>
    private readonly IConfigProvider _configProvider = configProvider;

    /// <summary>
    /// Xử lý truy vấn ChatWithAssistantQuery để tạo phản hồi trò chuyện.
    /// </summary>
    /// <param name="request">Truy vấn chứa tin nhắn người dùng và ID phiên.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa ChatResponse hoặc thông báo lỗi.</returns>
    /// <exception cref="Exception">Ném ra nếu có lỗi trong quá trình tạo phản hồi trò chuyện.</exception>
    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantQuery request, CancellationToken cancellationToken)
    {
        try
        {
            bool isFallback = false;
            ChatResponse? chatResponse = null;
            var systemPrompt = new ChatMessage
            {
                Role = "assistant",
                Content = @"Bạn là trợ lý AI của FamilyTree. BẮT BUỘC trả lời bằng TIẾNG VIỆT.
                - Chỉ trả lời các câu hỏi liên quan tới chức năng phần mềm, hướng dẫn sử dụng, cấu hình và troubleshooting.
                - Câu trả lời phải ngắn gọn, dễ hiểu, thân thiện, ví dụ có thể dùng bullet hoặc số nếu hướng dẫn nhiều bước."
            };

            // Proceed with RAG flow
            var embeddingSettings = _configProvider.GetSection<EmbeddingSettings>();
            var embeddingProvider = _embeddingProviderFactory.GetProvider(Enum.Parse<EmbeddingAIProvider>(embeddingSettings.Provider));
            var embeddingResult = await embeddingProvider.GenerateEmbeddingAsync(request.UserMessage, cancellationToken);

            if (!embeddingResult.IsSuccess)
                return Result<ChatResponse>.Failure($"Failed to generate embedding: {embeddingResult.Error}");

            var userEmbedding = embeddingResult.Value!;

            // 2. Search in vector store
            var vectorStoreSettings = _configProvider.GetSection<VectorStoreSettings>();
            var vectorStore = _vectorStoreFactory.CreateVectorStore(Enum.Parse<VectorStoreProviderType>(vectorStoreSettings.Provider));
            var silimarityResults = await vectorStore.QueryAsync(userEmbedding, vectorStoreSettings.TopK, [], cancellationToken);
            var chatSettings = _configProvider.GetSection<AIChatSettings>();
            var searchResults = silimarityResults.Where(s => s.Score > (chatSettings.ScoreThreshold / 100.0)).OrderByDescending(r => r.Score).ToList();

            // 3. Check if relevant context exists
            if (searchResults == null || searchResults.Count == 0)
            {
                isFallback = true;
                _logger.LogInformation("No relevant context found for user message: {UserMessage}. Using fallback prompt.", request.UserMessage);

                chatResponse = new ChatResponse
                {
                    Response = "Tôi không tìm thấy thông tin liên quan trong cơ sở dữ liệu. Bạn có câu hỏi nào khác về phần mềm không?",
                    SessionId = request.SessionId,
                    Model = isFallback ? "Fallback" : chatSettings.Provider.ToString(),
                    CreatedAt = DateTime.UtcNow
                };
                return Result<ChatResponse>.Success(chatResponse);
            }
            else
            {
                // 4. Construct augmented prompt
                var context = string.Join("\n\n", searchResults.Select(c => c.Content));
                var finalPromptContent = $"Context: {context}\n\nUser: {request.UserMessage}";
                // Construct messages list for the chat provider
                var messages = new List<ChatMessage>{
                    systemPrompt,
                    new() { Role = "user", Content = finalPromptContent }
                };

                // 5. Get chat response using the determined prompt
                var chatProvider = _chatProviderFactory.GetProvider(Enum.Parse<ChatAIProvider>(chatSettings.Provider));
                var responseContent = await chatProvider.GenerateResponseAsync(messages);

                chatResponse = new ChatResponse
                {
                    Response = responseContent,
                    SessionId = request.SessionId,
                    Model = isFallback ? "Fallback" : chatSettings.Provider.ToString(),
                    CreatedAt = DateTime.UtcNow
                };
            }
            return Result<ChatResponse>.Success(chatResponse);
        }
        catch (Exception ex)
        {
            return Result<ChatResponse>.Failure($"Failed to generate chat response: {ex.Message}");
        }
    }

}
