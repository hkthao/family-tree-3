using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AISettings;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Chat;

public class LocalAIProvider : IChatProvider
{
    private readonly LocalAISettings _localAISettings;

    public LocalAIProvider(IOptions<AIChatSettings> chatSettings)
    {
        _localAISettings = chatSettings.Value.Providers["LocalAI"] as LocalAISettings ?? throw new InvalidOperationException("LocalAI settings not found.");
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for LocalAI, now with access to _localAISettings.Endpoint
        return await Task.FromResult($"LocalAI responded to: {prompt} using endpoint {_localAISettings.Endpoint}");
    }
}
