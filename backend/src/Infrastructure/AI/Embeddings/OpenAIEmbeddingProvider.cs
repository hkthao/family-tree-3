using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Embeddings;

public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingSettings _settings;

    public string ProviderName => "OpenAI";
    public int MaxTextLength => _settings.OpenAI.MaxTextLength;

    public OpenAIEmbeddingProvider(IOptions<EmbeddingSettings> embeddingSettings)
    {
        _settings = embeddingSettings.Value;
    }

    public async Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.OpenAI.ApiKey))
        {
            return Result<float[]>.Failure("OpenAI API key is not configured.");
        }

        if (text.Length > MaxTextLength)
        {
            text = text[..MaxTextLength];
        }

        var embedding = new float[1024];
        for (int i = 0; i < 1024; i++)
        {
            embedding[i] = (float)Random.Shared.NextDouble();
        }
        await Task.Delay(100, cancellationToken); // Simulate API call delay
        return Result<float[]>.Success(embedding);
    }
}
