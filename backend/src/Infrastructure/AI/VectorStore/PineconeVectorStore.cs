using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pinecone;
using backend.Application.AI.VectorStore;

namespace backend.Infrastructure.AI.VectorStore;

public class PineconeVectorStore : IVectorStore
{
    private readonly ILogger<PineconeVectorStore> _logger;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly PineconeClient _pineconeClient;
    private readonly IndexClient _index;
    private readonly string _indexName;

    public PineconeVectorStore(ILogger<PineconeVectorStore> logger, IOptions<VectorStoreSettings> vectorStoreSettings)
    {
        _logger = logger;
        _vectorStoreSettings = vectorStoreSettings.Value;

        var pineconeSettings = _vectorStoreSettings.Pinecone;

        if (pineconeSettings == null)
        {
            throw new InvalidOperationException("Pinecone settings not found or invalid.");
        }

        var apiKey = pineconeSettings.ApiKey;
        _indexName = pineconeSettings.IndexName;

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(_indexName))
        {
            _logger.LogError("Pinecone configuration is missing or invalid. Check VectorStoreSettings:Pinecone:ApiKey, VectorStoreSettings:Pinecone:Environment, and VectorStoreSettings:Pinecone:IndexName in settings.");
            throw new InvalidOperationException("Pinecone configuration is missing or invalid.");
        }

        _pineconeClient = new PineconeClient(apiKey);
        _index = _pineconeClient.Index(_indexName);
    }

    public async Task UpsertAsync(TextChunk chunk, CancellationToken cancellationToken = default)
    {
        if (chunk.Embedding == null || chunk.Embedding.Length == 0)
        {
            _logger.LogWarning("Attempted to upsert chunk {ChunkId} without embedding. Skipping.", chunk.Id);
            return;
        }

        try
        {
            var vector = new Vector
            {
                Id = chunk.Id,
                Values = new ReadOnlyMemory<float>([.. chunk.Embedding]),
                Metadata = chunk.Metadata != null ? new Metadata(chunk.Metadata.ToDictionary(k => k.Key, v => (MetadataValue?)v.Value)) : null
            };

            var upsertRequest = new UpsertRequest { Vectors = [vector] };
            await _index.UpsertAsync(upsertRequest, null, cancellationToken);

            _logger.LogInformation("Successfully upserted chunk {ChunkId} to Pinecone index {IndexName}.", chunk.Id, _indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting chunk {ChunkId} to Pinecone index {IndexName}.", chunk.Id, _indexName);
            throw; // Re-throw to be handled by higher layers
        }
    }

    public async Task<List<TextChunk>> QueryAsync(float[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            throw new ArgumentException("Query embedding cannot be empty.");
        }

        try
        {
            var pineconeFilter = metadataFilter != null ? new Metadata(metadataFilter.ToDictionary(k => k.Key, v => (MetadataValue?)v.Value)) : null;

            var queryRequest = new QueryRequest
            {
                Vector = new ReadOnlyMemory<float>([.. queryEmbedding]),
                TopK = (uint)topK,
                IncludeValues = true,
                IncludeMetadata = true,
                Filter = pineconeFilter
            };

            var queryResponse = await _index.QueryAsync(queryRequest, cancellationToken: cancellationToken);

            var results = queryResponse.Matches?.Select(m => new TextChunk
            {
                Id = m.Id,
                Content = m.Metadata?.FirstOrDefault(md => md.Key == "content").Value?.ToString() ?? string.Empty, // Assuming content is stored in metadata
                Embedding = m.Values.HasValue ? m.Values.Value.ToArray() : [],
                Metadata = m.Metadata != null ? m.Metadata.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty) : new Dictionary<string, string>()
            }).ToList() ?? new List<TextChunk>();

            _logger.LogInformation("Successfully queried Pinecone index {IndexName} with TopK {TopK}. Found {Count} matches.", _indexName, topK, results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Pinecone index {IndexName}.", _indexName);
            throw; // Re-throw to be handled by higher layers
        }
    }
}