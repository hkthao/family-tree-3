using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AISettings;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Chat;

public class OpenAIChatProvider : IChatProvider
{
    private readonly OpenAISettings _openAISettings;

    public OpenAIChatProvider(IOptions<AIChatSettings> chatSettings)
    {
        _openAISettings = chatSettings.Value.Providers["OpenAI"] as OpenAISettings ?? throw new InvalidOperationException("OpenAI settings not found.");
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for OpenAI, now with access to _openAISettings.ApiKey and _openAISettings.Model
        return await Task.FromResult($"OpenAI responded to: {prompt} using model {_openAISettings.Model} with API Key {_openAISettings.ApiKey}");
    }
}
