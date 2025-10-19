using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.Embeddings
{
    public class EmbeddingProviderFactory : IEmbeddingProviderFactory
    {
            private readonly IServiceScopeFactory _serviceScopeFactory;
        
            public EmbeddingProviderFactory(IServiceScopeFactory serviceScopeFactory)
            {
                _serviceScopeFactory = serviceScopeFactory;
            }
        public IEmbeddingProvider GetProvider(EmbeddingAIProvider provider)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider; // Get the service provider for this scope

            return provider switch
            {
                EmbeddingAIProvider.OpenAI => serviceProvider.GetRequiredService<OpenAIEmbeddingProvider>(),
                EmbeddingAIProvider.Cohere => serviceProvider.GetRequiredService<CohereEmbeddingProvider>(),
                EmbeddingAIProvider.Local => serviceProvider.GetRequiredService<LocalEmbeddingProvider>(),
                _ => throw new ArgumentException($"Unsupported embedding provider: {provider}")
            };
        }
    }
}
