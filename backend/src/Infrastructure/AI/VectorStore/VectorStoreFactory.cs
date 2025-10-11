using backend.Application.AI.VectorStore;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.VectorStore
{
    public class VectorStoreFactory : IVectorStoreFactory
    {
        private readonly IOptions<VectorStoreSettings> _vectorStoreSettings;
        private readonly IEnumerable<IVectorStore> _providers;

        public VectorStoreFactory(IOptions<VectorStoreSettings> vectorStoreSettings, IEnumerable<IVectorStore> providers)
        {
            _vectorStoreSettings = vectorStoreSettings;
            _providers = providers;
        }

        public IVectorStore CreateVectorStore()
        {
            var providerType = _vectorStoreSettings.Value.VectorStoreProvider;

            var provider = _providers.FirstOrDefault(p => p.GetType().Name.StartsWith(providerType.ToString(), StringComparison.OrdinalIgnoreCase));

            if (provider == null)
            {
                throw new InvalidOperationException($"No vector store provider found for: {providerType}");
            }

            return provider;
        }
    }
}
