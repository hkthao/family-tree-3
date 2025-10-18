using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Embeddings;

public class CohereEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingSettings _settings;

    public string ProviderName => "Cohere";
    public int MaxTextLength => _settings.Cohere.MaxTextLength;

    public CohereEmbeddingProvider(IOptions<EmbeddingSettings> embeddingSettings)
    {
        _settings = embeddingSettings.Value;
    }

    public async Task<Result<double[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Cohere.ApiKey))
        {
            return Result<double[]>.Failure("Cohere API key is not configured.");
        }

        if (text.Length > MaxTextLength)
        {
            text = text[..MaxTextLength];
        }

        // TODO: Implement actual Cohere API call to generate embedding
        // For now, return a dummy embedding for demonstration
        await Task.Delay(100, cancellationToken); // Simulate API call delay
        return Result<double[]>.Success(new double[] { 0.4, 0.5, 0.6 });
    }
}
