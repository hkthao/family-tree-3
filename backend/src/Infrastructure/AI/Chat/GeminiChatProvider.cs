using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.AI.Chat;

public class GeminiChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly AIChatSettings _chatSettings;
    private readonly ILogger<GeminiChatProvider> _logger;

    public GeminiChatProvider(HttpClient httpClient, AIChatSettings chatSettings, ILogger<GeminiChatProvider> logger)
    {
        _httpClient = httpClient;
        _chatSettings = chatSettings;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for Gemini, now with access to _geminiSettings.ApiKey and _geminiSettings.Model
        return await Task.FromResult($"Gemini responded to: {prompt} using model {_chatSettings.Gemini.Model} with API Key {_chatSettings.Gemini.ApiKey}");
    }
}
