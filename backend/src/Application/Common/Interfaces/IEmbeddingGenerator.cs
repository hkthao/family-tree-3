using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IEmbeddingGenerator
{
    Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}
