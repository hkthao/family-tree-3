using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.Chat;

public class ChatProviderFactory : IChatProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ChatProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IChatProvider GetProvider(ChatAIProvider provider)
    {
        return provider switch
        {
            ChatAIProvider.Local => _serviceProvider.GetRequiredService<LocalChatProvider>(),
            ChatAIProvider.Gemini => _serviceProvider.GetRequiredService<GeminiChatProvider>(),
            ChatAIProvider.OpenAI => _serviceProvider.GetRequiredService<OpenAIChatProvider>(),
            _ => throw new InvalidOperationException($"No file chat AI provider configured for: {provider}")
        };
    }
}
