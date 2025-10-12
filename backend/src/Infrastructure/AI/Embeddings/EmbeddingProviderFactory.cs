using backend.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.Embeddings
{
    public class EmbeddingProviderFactory : IEmbeddingProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public EmbeddingProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IEmbeddingProvider GetProvider(string providerName)
        {
            return providerName.ToLowerInvariant() switch
            {
                "openai" => _serviceProvider.GetRequiredService<OpenAIEmbeddingProvider>(),
                "cohere" => _serviceProvider.GetRequiredService<CohereEmbeddingProvider>(),
                "local" => _serviceProvider.GetRequiredService<LocalEmbeddingProvider>(),
                _ => throw new ArgumentException($"Unsupported embedding provider: {providerName}")
            };
        }
    }
}