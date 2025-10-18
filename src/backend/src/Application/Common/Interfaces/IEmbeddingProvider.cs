using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IEmbeddingProvider
{
    string ProviderName { get; }
    int MaxTextLength { get; }
    Task<Result<double[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
