using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Chat;

public class OpenAIProvider : ILLMProvider
{
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for OpenAI
        return await Task.FromResult($"OpenAI responded to: {prompt}");
    }
}
