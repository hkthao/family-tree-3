using backend.Application.AI.Embeddings;
using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.Embeddings;

public class EmbeddingProviderFactory : IEmbeddingProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EmbeddingProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmbeddingProvider CreateProvider(EmbeddingProvider provider)
    {
        return provider switch
        {
            EmbeddingProvider.Local => _serviceProvider.GetRequiredService<LocalEmbeddingProvider>(),
            EmbeddingProvider.Cohere => _serviceProvider.GetRequiredService<CohereEmbeddingProvider>(),
            EmbeddingProvider.OpenAI => _serviceProvider.GetRequiredService<OpenAIEmbeddingProvider>(),
            _ => throw new InvalidOperationException($"No file Embedding provider configured for: {provider}")
        };
    }
}
