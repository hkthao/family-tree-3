using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.Chat;

public class LLMProviderFactory : ILLMProviderFactory
{
    private readonly IOptions<ChatSettings> _chatSettings;
    private readonly IEnumerable<ILLMProvider> _providers;

    public LLMProviderFactory(IOptions<ChatSettings> chatSettings, IEnumerable<ILLMProvider> providers)
    {
        _chatSettings = chatSettings;
        _providers = providers;
    }

    public ILLMProvider GetProvider()
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
