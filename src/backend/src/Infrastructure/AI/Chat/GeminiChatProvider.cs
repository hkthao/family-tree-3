using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;

namespace backend.Infrastructure.AI.Chat;

public class GeminiChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiChatProvider> _logger;
    private readonly IConfigProvider _configProvider;

    public GeminiChatProvider(HttpClient httpClient, IConfigProvider configProvider, ILogger<GeminiChatProvider> logger)
    {
        _httpClient = httpClient;
        _configProvider = configProvider;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(List<ChatMessage> messages)
    {
        var chatSettings = _configProvider.GetSection<AIChatSettings>();
        // Dummy implementation for Gemini
        var concatenatedMessages = string.Join("\n", messages.Select(m => $"{m.Role}: {m.Content}"));
        _logger.LogInformation("Gemini received messages: {Messages}", concatenatedMessages);
        return await Task.FromResult($"Gemini responded to: {concatenatedMessages} using model {chatSettings.Gemini.Model} with API Key {chatSettings.Gemini.ApiKey}");
    }
}
