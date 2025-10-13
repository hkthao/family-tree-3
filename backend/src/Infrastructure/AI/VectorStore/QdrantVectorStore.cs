using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using backend.Application.AI.VectorStore;

namespace backend.Infrastructure.AI.VectorStore;

public class QdrantVectorStore : IVectorStore
{
    private readonly ILogger<QdrantVectorStore> _logger;
    private readonly VectorStoreSettings _vectorStoreSettings;
    private readonly QdrantClient _qdrantClient;
    private readonly string _collectionName;
    private readonly int _vectorSize;

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
        _collectionName = qdrantSettings.CollectionName;
        _vectorSize = qdrantSettings.VectorSize;

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(_collectionName))
        {
            _logger.LogError("Qdrant configuration is missing or invalid. Check VectorStoreSettings:Qdrant:Host, VectorStoreSettings:Qdrant:CollectionName in settings.");
            throw new InvalidOperationException("Qdrant configuration is missing or invalid.");
        }

        _qdrantClient = new QdrantClient(host: host, apiKey: apiKey, https: true);

        // Ensure collection exists
        Task.Run(EnsureCollectionExistsAsync).Wait();
    }

    private async Task EnsureCollectionExistsAsync()
    {
        var collectionExists = await _qdrantClient.CollectionExistsAsync(_collectionName);
        if (!collectionExists)
        {
            _logger.LogInformation("Creating Qdrant collection {CollectionName} with vector size {VectorSize}.", _collectionName, _vectorSize);
            await _qdrantClient.CreateCollectionAsync(
                _collectionName,
                new VectorParams { Size = (ulong)_vectorSize, Distance = Distance.Cosine }
            );
        }
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
            var points = new List<PointStruct>
            {
                new() {
                    Id = new PointId { Uuid = chunk.Id },
                    Vectors = new Vectors {
                        Vector = new Vector { Data = { chunk.Embedding.Select(e => (float)e) } }
                    },
                    Payload = {
                        { "content", chunk.Content },
                        { "fileName", chunk.Metadata.GetValueOrDefault("fileName", string.Empty) },
                        { "fileId", chunk.Metadata.GetValueOrDefault("fileId", string.Empty) },
                        { "familyId", chunk.Metadata.GetValueOrDefault("familyId", string.Empty) },
                        { "category", chunk.Metadata.GetValueOrDefault("category", string.Empty) },
                        { "createdBy", chunk.Metadata.GetValueOrDefault("createdBy", string.Empty) },
                        { "createdAt", chunk.Metadata.GetValueOrDefault("createdAt", string.Empty) },
                        { "page", chunk.Metadata.GetValueOrDefault("page", string.Empty) }
                    }
                }
            };

            await _qdrantClient.UpsertAsync(_collectionName, points, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully upserted chunk {ChunkId} to Qdrant collection {CollectionName}.", chunk.Id, _collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting chunk {ChunkId} to Qdrant collection {CollectionName}.", chunk.Id, _collectionName);
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
            var searchPoints = await _qdrantClient.SearchAsync(
                collectionName: _collectionName,
                vector: new ReadOnlyMemory<float>(queryEmbedding),
                limit: (ulong)topK,
                payloadSelector: new WithPayloadSelector { Enable = true },
                filter: CreateQdrantFilter(metadataFilter),
                cancellationToken: cancellationToken
            );

            var results = new List<TextChunk>();
            foreach (var foundPoint in searchPoints)
            {
                var payload = foundPoint.Payload;
                results.Add(new TextChunk
                {
                    Id = foundPoint.Id.Uuid,
                    Content = payload.TryGetValue("content", out var contentValue) ? contentValue.StringValue ?? string.Empty : string.Empty,
                    Embedding = foundPoint.Vectors?.Vector?.Data?.Select(e => (float)e).ToArray() ?? [],
                    Score = foundPoint.Score,
                    Metadata = payload.ToDictionary(
                        p => p.Key,
                        p => p.Value.StringValue ?? string.Empty
                    )
                });
            }

            _logger.LogInformation("Successfully queried Qdrant collection {CollectionName} with TopK {TopK}. Found {Count} matches.", _collectionName, topK, results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Qdrant collection {CollectionName}.", _collectionName);
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
