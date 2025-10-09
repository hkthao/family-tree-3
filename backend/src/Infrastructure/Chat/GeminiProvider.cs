using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Chat;

public class GeminiProvider : ILLMProvider
{
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for Gemini
        return await Task.FromResult($"Gemini responded to: {prompt}");
    }
}
