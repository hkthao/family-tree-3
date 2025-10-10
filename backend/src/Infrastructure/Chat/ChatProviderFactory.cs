using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Chat;

public class ChatProviderFactory : IChatProviderFactory
{
    private readonly IOptions<AIChatSettings> _chatSettings;
    private readonly IEnumerable<IChatProvider> _providers;

    public ChatProviderFactory(IOptions<AIChatSettings> chatSettings, IEnumerable<IChatProvider> providers)
    {
        _chatSettings = chatSettings;
        _providers = providers;
    }

    public IChatProvider GetProvider()
    {
        var providerName = _chatSettings.Value.Provider.ToString();
        var provider = _providers.FirstOrDefault(p => p.GetType().Name.StartsWith(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            throw new InvalidOperationException($"No LLM provider found for: {providerName}");
        }

        return provider;
    }
}
