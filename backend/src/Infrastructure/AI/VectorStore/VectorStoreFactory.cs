using backend.Application.AI.VectorStore;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.VectorStore
{
    public class VectorStoreFactory : IVectorStoreFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public VectorStoreFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IVectorStore CreateVectorStore(VectorStoreProviderType provider)
        {
            return provider switch
            {
                VectorStoreProviderType.Pinecone => _serviceProvider.GetRequiredService<PineconeVectorStore>(),
                _ => throw new InvalidOperationException($"No vector store provider configured for: {provider}")
            };
        }

    }
}
