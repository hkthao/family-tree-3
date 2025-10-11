using backend.Application.AI.Embeddings;
using backend.Application.Common.Models;

namespace backend.Application.AI.Embeddings;

public interface IEmbeddingProviderFactory
{
    Result<IEmbeddingProvider> GetProvider(string providerName);
}
