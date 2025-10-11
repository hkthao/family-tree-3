using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Infrastructure.AI.Chat
{
    public class OpenAIChatProvider : IChatProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AIChatSettings _chatSettings;

        public OpenAIChatProvider(HttpClient httpClient, AIChatSettings chatSettings)
        {
            _httpClient = httpClient;
            _chatSettings = chatSettings;
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            return await Task.FromResult($"OpenAI responded to: {prompt} using model {_chatSettings.OpenAI.Model} with API Key {_chatSettings.OpenAI.ApiKey}");
        }
    }
}
