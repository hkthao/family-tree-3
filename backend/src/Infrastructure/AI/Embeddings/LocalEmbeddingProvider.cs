using backend.Application.AI.Embeddings;
using backend.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace backend.Infrastructure.AI.Embeddings;

public class LocalEmbeddingProvider : IEmbeddingProvider
{
    private readonly EmbeddingSettings _settings;

    public string ProviderName => "Local";
    public int MaxTextLength => _settings.Local.MaxTextLength;

    public LocalEmbeddingProvider(IOptions<EmbeddingSettings> embeddingSettings)
    {
        _settings = embeddingSettings.Value;
    }

    public async Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (text.Length > MaxTextLength)
        {
            text = text[..MaxTextLength];
        }

        // Simulate local model processing
        await Task.Delay(50, cancellationToken); // Simulate some processing time
        return Result<float[]>.Success(new float[] { 0.7f, 0.8f, 0.9f });
    }
}
