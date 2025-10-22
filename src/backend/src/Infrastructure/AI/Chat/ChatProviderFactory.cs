using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.Chat;

public class ChatProviderFactory(IServiceScopeFactory serviceScopeFactory) : IChatProviderFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public IChatProvider GetProvider(ChatAIProvider provider)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider; // Get the service provider for this scope

        return provider switch
        {
            ChatAIProvider.Gemini => serviceProvider.GetRequiredService<GeminiChatProvider>(),
            ChatAIProvider.OpenAI => serviceProvider.GetRequiredService<OpenAIChatProvider>(),
            ChatAIProvider.Local => serviceProvider.GetRequiredService<LocalChatProvider>(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), $"Not supported chat AI provider: {provider}")
        };
    }
}
