using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.VectorStore;

public class VectorStoreFactory : IVectorStoreFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public VectorStoreFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public IVectorStore CreateVectorStore(VectorStoreProviderType provider)
    {
        var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider; // Get the service provider for this scope

        return provider switch
        {
            VectorStoreProviderType.Pinecone => serviceProvider.GetRequiredService<PineconeVectorStore>(),
            VectorStoreProviderType.InMemory => serviceProvider.GetRequiredService<InMemoryVectorStore>(),
            VectorStoreProviderType.Qdrant => serviceProvider.GetRequiredService<QdrantVectorStore>(),
            _ => throw new InvalidOperationException($"No vector store provider configured for: {provider}")
        };
    }

}
