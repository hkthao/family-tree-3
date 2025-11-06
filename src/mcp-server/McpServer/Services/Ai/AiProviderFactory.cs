using McpServer.Services.Ai.Providers;

namespace McpServer.Services.Ai
{
    /// <summary>
    /// Factory để tạo và trả về các nhà cung cấp AI Assistant khác nhau.
    /// </summary>
    public class AiProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AiProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Lấy một thể hiện của nhà cung cấp AI Assistant dựa trên tên.
        /// </summary>
        /// <param name="providerName">Tên của nhà cung cấp AI (ví dụ: "Gemini", "OpenAI", "LocalLLM").</param>
        /// <returns>Một thể hiện của IAiProvider.</returns>
        /// <exception cref="ArgumentException">Ném ra nếu không tìm thấy nhà cung cấp AI.</exception>
        public virtual IAiProvider GetProvider(string providerName)
        {
            return providerName.ToLowerInvariant() switch
            {
                // "gemini" => _serviceProvider.GetRequiredService<GeminiProvider>(),
                // "openai" => _serviceProvider.GetRequiredService<OpenAiProvider>(),
                "localllm" => _serviceProvider.GetRequiredService<LocalLlmProvider>(),
                _ => throw new ArgumentException($"AI provider '{providerName}' not found.")
            };
        }
    }
}