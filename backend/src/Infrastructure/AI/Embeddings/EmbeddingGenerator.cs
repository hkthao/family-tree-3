using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Infrastructure.AI.Embeddings;

public class EmbeddingGenerator : IEmbeddingGenerator
{
    public Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // Dummy implementation: return a fixed dummy embedding
        // In a real scenario, this would call an external embedding model API
        var dummyEmbedding = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f }; // Example dummy embedding
        return Task.FromResult(Result<float[]>.Success(dummyEmbedding));
    }
}
