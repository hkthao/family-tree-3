using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Infrastructure.AI.Embeddings
{
    public class LocalEmbeddingProvider : IEmbeddingProvider
    {
        public string ProviderName => "Local";
        public int MaxTextLength => 1000; // Arbitrary length for local testing

        public Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult(Result<float[]>.Failure("Text for embedding cannot be empty."));
            }

            // Generate a dummy embedding for local testing
            // In a real scenario, this would call a local model or return a fixed vector
            float[] dummyEmbedding = new float[1536]; // Common embedding dimension
            Random rand = new Random();
            for (int i = 0; i < dummyEmbedding.Length; i++)
            {
                dummyEmbedding[i] = (float)rand.NextDouble();
            }

            return Task.FromResult(Result<float[]>.Success(dummyEmbedding));
        }
    }
}