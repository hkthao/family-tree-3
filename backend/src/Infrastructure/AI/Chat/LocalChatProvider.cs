using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.AI.Chat
{
    public class LocalChatProvider : IChatProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AIChatSettings _chatSettings;
        private readonly ILogger<LocalChatProvider> _logger;

        public LocalChatProvider(HttpClient httpClient, AIChatSettings chatSettings, ILogger<LocalChatProvider> logger)
        {
            _httpClient = httpClient;
            _chatSettings = chatSettings;
            _logger = logger;
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            // Dummy implementation for LocalAI, now with access to _localAISettings.Endpoint
            return await Task.FromResult($"LocalAI responded to: {prompt} using endpoint {_chatSettings.Local.Endpoint}");
        }
    }
}
