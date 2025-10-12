using backend.Application.Common.Interfaces;
using backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.AI.VectorStore;

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
            VectorStoreProviderType.InMemory => _serviceProvider.GetRequiredService<InMemoryVectorStore>(),
            _ => throw new InvalidOperationException($"No vector store provider configured for: {provider}")
        };
    }

}
