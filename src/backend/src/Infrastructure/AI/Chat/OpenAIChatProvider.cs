using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;

namespace backend.Infrastructure.AI.Chat;

public class OpenAIChatProvider : IChatProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfigProvider _configProvider;

    public OpenAIChatProvider(HttpClient httpClient, IConfigProvider configProvider)
    {
        _httpClient = httpClient;
        _configProvider = configProvider;
    }

    public async Task<string> GenerateResponseAsync(List<ChatMessage> messages)
    {
        var chatSettings = _configProvider.GetSection<AIChatSettings>();
        // Dummy implementation for OpenAI
        var concatenatedMessages = string.Join("\n", messages.Select(m => $"{m.Role}: {m.Content}"));
        return await Task.FromResult($"OpenAI responded to: {concatenatedMessages} using model {chatSettings.OpenAI.Model} with API Key {chatSettings.OpenAI.ApiKey}");
    }
}
