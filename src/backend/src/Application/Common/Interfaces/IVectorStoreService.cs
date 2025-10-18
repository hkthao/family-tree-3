using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IVectorStoreService
{
    Task<Result<Unit>> SaveVectorAsync(List<float> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken);
    // Add other vector store related methods (e.g., SearchVectors)
}
