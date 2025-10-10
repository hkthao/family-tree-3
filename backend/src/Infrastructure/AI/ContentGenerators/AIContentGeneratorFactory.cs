using backend.Application.Common.Interfaces;
using backend.Application.AI.ContentGenerators;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.ContentGenerators;

public class AIContentGeneratorFactory : IAIContentGeneratorFactory
{
    private readonly IOptions<AIContentGeneratorSettings> _aiContentGeneratorSettings;
    private readonly IEnumerable<IAIContentGenerator> _providers;

    public AIContentGeneratorFactory(IOptions<AIContentGeneratorSettings> aiContentGeneratorSettings, IEnumerable<IAIContentGenerator> providers)
    {
        _aiContentGeneratorSettings = aiContentGeneratorSettings;
        _providers = providers;
    }

    public IAIContentGenerator GetContentGenerator()
    {
        var providerName = _aiContentGeneratorSettings.Value.Provider.ToString();
        var provider = _providers.FirstOrDefault(p => p.GetType().Name.StartsWith(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            throw new InvalidOperationException($"No AI content generator found for: {providerName}");
        }

        return provider;
    }
}