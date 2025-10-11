using backend.Application.Common.Interfaces;
using backend.Application.AI.VectorStore;

namespace backend.Application.AI.Chat
{
    public class ChatService : IChatService
    {
        private readonly IVectorStore _vectorStore;
        private readonly IEmbeddingService _embeddingService;

        public ChatService(
            IVectorStore vectorStore,
            IEmbeddingService embeddingService)
        {
            _vectorStore = vectorStore;
            _embeddingService = embeddingService;
        }

        public async Task<ChatResponse> SendMessageAsync(string userMessage, string? sessionId = null)
        {
            // Generate embeddings for the user message
            var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(userMessage);
            if (!embeddingResult.IsSuccess)
            {
                return new ChatResponse { Response = "Error generating embeddings." };
            }

            // Query VectorStore for semantically relevant context
            var vectorQuery = new VectorQuery
            {
                Vector = embeddingResult.Value!,
                TopK = 5 // Retrieve top 5 relevant documents
            };
            var queryResult = await _vectorStore.QueryAsync(vectorQuery);

            var context = new List<string>();
            if (queryResult.IsSuccess && queryResult.Value != null)
            {
                context = queryResult.Value.Select(d => d.Content).ToList();
            }

            return await SendMessageWithContextAsync(userMessage, context, sessionId);
        }

        public async Task<ChatResponse> SendMessageWithContextAsync(string userMessage, IEnumerable<string> context, string? sessionId = null)
        {
            // Construct a structured prompt
            var promptBuilder = new System.Text.StringBuilder();
            promptBuilder.AppendLine("You are a helpful family knowledge assistant.");
            promptBuilder.AppendLine("Use the context below to answer user questions naturally.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Context:");
            if (context != null && context.Any())
            {
                foreach (var item in context)
                {
                    promptBuilder.AppendLine(item);
                }
            }
            else
            {
                promptBuilder.AppendLine("No specific context available.");
            }
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"User question: {userMessage}");

            var prompt = promptBuilder.ToString();

            return await Task.FromResult(new ChatResponse());

            // Call the LLM provider
            // var chatProvider = _chatProviderFactory.CreateChatProvider();
            // var response = await chatProvider.GenerateResponseAsync(userMessage);

            // return new ChatResponse
            // {
            //     Response = response,
            //     Context = context?.ToList() ?? []
            // };
        }
    }
}
