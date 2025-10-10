using backend.Application.Common.Interfaces;
using backend.Application.Common.Models.AISettings;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Chat;

public class GeminiProvider : IChatProvider
{
    private readonly GeminiSettings _geminiSettings;

    public GeminiProvider(IOptions<AIChatSettings> chatSettings)
    {
        _geminiSettings = chatSettings.Value.Providers["Gemini"] as GeminiSettings ?? throw new InvalidOperationException("Gemini settings not found.");
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for Gemini, now with access to _geminiSettings.ApiKey and _geminiSettings.Model
        return await Task.FromResult($"Gemini responded to: {prompt} using model {_geminiSettings.Model} with API Key {_geminiSettings.ApiKey}");
    }
}
