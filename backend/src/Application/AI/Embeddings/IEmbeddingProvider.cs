using backend.Application.Common.Models;

namespace backend.Application.AI.Embeddings;

public interface IEmbeddingProvider
{
    string ProviderName { get; }
    int MaxTextLength { get; }
    Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
