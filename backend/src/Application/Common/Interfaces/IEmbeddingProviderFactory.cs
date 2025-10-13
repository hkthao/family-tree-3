using backend.Domain.Enums;

namespace backend.Application.Common.Interfaces;

public interface IEmbeddingProviderFactory
{
    IEmbeddingProvider GetProvider(EmbeddingAIProvider provider);
}