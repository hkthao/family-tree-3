using backend.Application.Common.Interfaces;
using backend.Domain.Enums;

namespace backend.Application.AI.Embeddings
{
    public interface IEmbeddingProviderFactory
    {
        IEmbeddingProvider CreateProvider(EmbeddingProvider provider);
    }
}
