using backend.Application.AI.VectorStore;
using backend.Application.Common.Models;
using backend.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using backend.Application.AI.Embeddings;

namespace backend.Infrastructure.AI.Embeddings;

public class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingSettings _embeddingSettings;
    private readonly IEmbeddingProviderFactory _embeddingProviderFactory;
    private readonly IVectorStoreFactory _vectorStoreFactory;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        IOptions<EmbeddingSettings> embeddingSettings,
        IEmbeddingProviderFactory embeddingProviderFactory,
        IVectorStoreFactory vectorStoreFactory,
        ILogger<EmbeddingService> logger)
    {
        _embeddingSettings = embeddingSettings.Value;
        _embeddingProviderFactory = embeddingProviderFactory;
        _vectorStoreFactory = vectorStoreFactory;
        _logger = logger;
    }

    public async Task<Result<float[]>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var providerResult = _embeddingProviderFactory.GetProvider(_embeddingSettings.Provider.ToString());

        if (!providerResult.IsSuccess)
        {
            _logger.LogError("Failed to get embedding provider: {Error}", providerResult.Error);
            return Result<float[]>.Failure(providerResult.Error ?? "Unknown error getting embedding provider.");
        }

        var provider = providerResult.Value!;

        _logger.LogInformation("Using embedding provider: {ProviderName}", _embeddingSettings.Provider.ToString());

        // Truncate text if it exceeds the provider's max length
        if (text.Length > provider.MaxTextLength)
        {
            _logger.LogWarning("Text length ({TextLength}) exceeds maximum allowed ({MaxTextLength}) for provider {ProviderName}. Truncating.", text.Length, provider.MaxTextLength, provider.ProviderName);
            text = text[..provider.MaxTextLength];
        }

        return await provider.GenerateEmbeddingAsync(text, cancellationToken);
    }

    // Example method for upserting/querying vectors, demonstrating VectorStoreFactory usage
    public async Task<Result> UpsertVectorAsync(string id, float[] vector, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        var vectorStore = _vectorStoreFactory.CreateVectorStore();
        return await vectorStore.UpsertVectorAsync(id, vector, metadata, cancellationToken);
    }

    public async Task<Result<List<string>>> QueryNearestVectorsAsync(float[] vector, int topK, CancellationToken cancellationToken = default)
    {
        var vectorStore = _vectorStoreFactory.CreateVectorStore();
        return await vectorStore.QueryNearestVectorsAsync(vector, topK, cancellationToken);
    }
}
