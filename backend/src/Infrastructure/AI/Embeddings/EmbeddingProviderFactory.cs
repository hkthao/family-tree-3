using backend.Application.AI.Embeddings;
using backend.Application.Common.Models;

namespace backend.Infrastructure.AI.Embeddings
{
    public class EmbeddingProviderFactory : IEmbeddingProviderFactory
    {
        private readonly IEnumerable<IEmbeddingProvider> _providers;

        public EmbeddingProviderFactory(IEnumerable<IEmbeddingProvider> providers)
        {
            _providers = providers;
        }

        public Result<IEmbeddingProvider> GetProvider(string providerName)
        {
            var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            if (provider == null)
            {
                return Result<IEmbeddingProvider>.Failure($"No embedding provider found for: {providerName}");
            }
            return Result<IEmbeddingProvider>.Success(provider);
        }
    }
}
