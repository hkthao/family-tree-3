using backend.Application.VectorStore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.VectorStore;

public class VectorStoreFactory : IVectorStoreFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<EmbeddingSettings> _embeddingSettings;

    public VectorStoreFactory(IServiceProvider serviceProvider, IOptions<EmbeddingSettings> embeddingSettings)
    {
        _serviceProvider = serviceProvider;
        _embeddingSettings = embeddingSettings;
    }

    public IVectorStore CreateVectorStore()
    {
        var provider = _embeddingSettings.Value.VectorStoreProvider.ToString();

        return provider switch
        {
            "Pinecone" => _serviceProvider.GetRequiredService<PineconeVectorStore>(),
            _ => throw new InvalidOperationException($"Vector store provider '{provider}' is not supported.")
        };
    }
}
