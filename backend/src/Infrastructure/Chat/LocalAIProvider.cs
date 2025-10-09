using backend.Application.Common.Interfaces;

namespace backend.Infrastructure.Chat;

public class LocalAIProvider : ILLMProvider
{
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        // Dummy implementation for LocalAI
        return await Task.FromResult($"LocalAI responded to: {prompt}");
    }
}
