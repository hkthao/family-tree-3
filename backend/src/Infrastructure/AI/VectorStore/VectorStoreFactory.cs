using backend.Application.AI.VectorStore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using backend.Application.AI.Embeddings;

namespace backend.Infrastructure.AI.VectorStore;

public class VectorStoreFactory : IVectorStoreFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<VectorStoreSettings> _vectorStoreSettings;

    public VectorStoreFactory(IServiceProvider serviceProvider, IOptions<VectorStoreSettings> vectorStoreSettings)
    {
        _serviceProvider = serviceProvider;
        _vectorStoreSettings = vectorStoreSettings;
    }

    public IVectorStore CreateVectorStore()
    {
        var provider = _vectorStoreSettings.Value.VectorStoreProvider.ToString();

        return provider switch
        {
            "Pinecone" => _serviceProvider.GetRequiredService<PineconeVectorStore>(),
            _ => throw new InvalidOperationException($"Vector store provider '{provider}' is not supported.")
        };
    }
}
