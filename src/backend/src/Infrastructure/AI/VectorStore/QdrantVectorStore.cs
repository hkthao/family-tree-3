using backend.Application.AI.VectorStore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace backend.Infrastructure.AI.VectorStore;

public class QdrantVectorStore : IVectorStore
{
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly QdrantClient _qdrantClient;
    private readonly string _defaultCollectionName; // Renamed from _collectionName
    private readonly int _defaultVectorSize; // Renamed from _vectorSize

    public QdrantVectorStore(ILogger<QdrantVectorStore> logger, IOptions<VectorStoreSettings> vectorStoreSettings)
    {
        _logger = logger;
        _vectorStoreSettings = vectorStoreSettings.Value;

        var qdrantSettings = _vectorStoreSettings.Qdrant;

        if (qdrantSettings == null)
        {
            throw new InvalidOperationException("Qdrant settings not found or invalid.");
        }

        var host = qdrantSettings.Host;
        var apiKey = qdrantSettings.ApiKey;
        _defaultCollectionName = qdrantSettings.CollectionName;
        _defaultVectorSize = qdrantSettings.VectorSize;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(_defaultCollectionName))
        {
            _logger.LogError("Qdrant configuration is missing or invalid. Check VectorStoreSettings:Qdrant:Host, VectorStoreSettings:Qdrant:CollectionName in settings.");
            throw new InvalidOperationException("Qdrant configuration is missing or invalid.");
        }

        _qdrantClient = new QdrantClient(host: host, apiKey: apiKey, https: true);

        // Ensure default collection exists
        Task.Run(() => EnsureCollectionExistsAsync(_defaultCollectionName, _defaultVectorSize)).Wait();
    }

    private async Task EnsureCollectionExistsAsync(string collectionName, int vectorSize)
    {
        var collectionExists = await _qdrantClient.CollectionExistsAsync(collectionName);
        if (!collectionExists)
        {
            _logger.LogInformation("Creating Qdrant collection {CollectionName} with vector size {VectorSize}. (Host: {Host})", collectionName, vectorSize, _vectorStoreSettings.Qdrant.Host);
            await _qdrantClient.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine }
            );
        }
    }

    public async Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        await UpsertAsync(embedding, metadata, _defaultCollectionName, _defaultVectorSize, cancellationToken);
    }

    public async Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, string collectionName, int embeddingDimension, CancellationToken cancellationToken = default)
    {
        if (embedding == null || !embedding.Any())
        {
            _logger.LogWarning("Attempted to upsert without embedding. Skipping.");
            return;
        }

        try
        {
            await EnsureCollectionExistsAsync(collectionName, embeddingDimension); // Ensure specific collection exists

            var pointId = Guid.NewGuid().ToString(); // Generate a unique ID for the point

            var payload = new Dictionary<string, Value>();
            foreach (var item in metadata)
            {
                payload.Add(item.Key, item.Value);
            }

            var points = new List<PointStruct>
            {
                new() {
                    Id = new PointId { Uuid = pointId },
                    Vectors = new Vectors {
                        Vector = new Vector { Data = { embedding.Select(e => (float)e) } }
                    },
                    Payload = { payload }
                }
            };

            await _qdrantClient.UpsertAsync(collectionName, points, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully upserted vector {PointId} to Qdrant collection {CollectionName}. (Dimension: {EmbeddingDimension})", pointId, collectionName, embeddingDimension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting vector to Qdrant collection {CollectionName}. (Dimension: {EmbeddingDimension})", collectionName, embeddingDimension);
            throw; // Re-throw to be handled by higher layers
        }
    }

    public async Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default)
    {
        return await QueryAsync(queryEmbedding, topK, metadataFilter, _defaultCollectionName, cancellationToken);
    }

    public async Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, string collectionName, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            throw new ArgumentException("Query embedding cannot be empty.");
        }

        try
        {
            var searchPoints = await _qdrantClient.SearchAsync(
                collectionName: collectionName, // Use specific collectionName
                vector: new ReadOnlyMemory<float>(queryEmbedding.Select(e => (float)e).ToArray()),
                limit: (ulong)topK,
                payloadSelector: new WithPayloadSelector { Enable = true },
                filter: CreateQdrantFilter(metadataFilter),
                cancellationToken: cancellationToken
            );

            var results = new List<VectorStoreQueryResult>();
            foreach (var foundPoint in searchPoints)
            {
                var payload = foundPoint.Payload;
                results.Add(new VectorStoreQueryResult
                {
                    Id = foundPoint.Id.Uuid,
                    Embedding = foundPoint.Vectors?.Vector?.Data?.Select(e => (double)e).ToList() ?? new List<double>(),
                    Score = foundPoint.Score,
                    Metadata = payload.ToDictionary(
                        p => p.Key,
                        p => p.Value.StringValue ?? string.Empty
                    ),
                    Content = payload.TryGetValue("Content", out var contentValue) ? contentValue.StringValue ?? string.Empty : string.Empty
                });
            }

            _logger.LogInformation("Successfully queried Qdrant collection {CollectionName} with TopK {TopK}. Found {Count} matches.", collectionName, topK, results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Qdrant collection {CollectionName}. (Host: {Host})", collectionName, _vectorStoreSettings.Qdrant.Host);
            throw; // Re-throw to be handled by higher layers
        }
    }

    private Filter? CreateQdrantFilter(Dictionary<string, string> metadataFilter)
    {
        if (metadataFilter == null || !metadataFilter.Any())
        {
            return null;
        }

        var mustConditions = new List<Condition>();
        foreach (var filter in metadataFilter)
        {
            mustConditions.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = filter.Key,
                    Match = new Match
                    {
                        Text = filter.Value
                    }
                }
            });
        }

        return new Filter { Must = { mustConditions } };
    }
}
