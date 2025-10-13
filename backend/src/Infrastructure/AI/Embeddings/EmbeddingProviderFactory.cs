using backend.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using backend.Domain.Enums;

namespace backend.Infrastructure.AI.Embeddings
{
    public class EmbeddingProviderFactory : IEmbeddingProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public EmbeddingProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEmbeddingProvider GetProvider(EmbeddingAIProvider provider)
        {
            return provider switch
            {
                EmbeddingAIProvider.OpenAI => _serviceProvider.GetRequiredService<OpenAIEmbeddingProvider>(),
                EmbeddingAIProvider.Cohere => _serviceProvider.GetRequiredService<CohereEmbeddingProvider>(),
                EmbeddingAIProvider.Local => _serviceProvider.GetRequiredService<LocalEmbeddingProvider>(),
                _ => throw new ArgumentException($"Unsupported embedding provider: {provider}")
            };
        }
    }
}