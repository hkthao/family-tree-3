using backend.Application.VectorStore;
using Microsoft.Extensions.DependencyInjection;

namespace backend.Infrastructure.VectorStore;

public class VectorStoreFactory : IVectorStoreFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IVectorStoreSettings _settings;

    public VectorStoreFactory(IServiceProvider serviceProvider, IVectorStoreSettings settings)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
    }

    public IVectorStore CreateVectorStore()
    {
        var provider = _settings.Provider;

        return provider switch
        {
            "Pinecone" => _serviceProvider.GetRequiredService<PineconeVectorStore>(),
            _ => throw new InvalidOperationException($"Vector store provider '{provider}' is not supported.")
        };
    }
}
