namespace backend.Application.Common.Interfaces;

public interface IEmbeddingProviderFactory
{
    IEmbeddingProvider GetProvider(string providerName);
}