using backend.Application.AI.VectorStore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;
using Pinecone;

namespace backend.Infrastructure.AI.VectorStore;

public class PineconeVectorStore : IVectorStore
{
    private readonly ILogger<PineconeVectorStore> _logger;
    private readonly IConfigProvider _configProvider;
    private readonly PineconeClient _pineconeClient;
    private readonly string _defaultIndexName; // Renamed from _indexName

    public PineconeVectorStore(ILogger<PineconeVectorStore> logger, IConfigProvider configProvider)
    {
        _logger = logger;
        _configProvider = configProvider;

        var vectorStoreSettings = _configProvider.GetSection<VectorStoreSettings>();
        var pineconeSettings = vectorStoreSettings.Pinecone;

        if (pineconeSettings == null)
        {
            throw new InvalidOperationException("Pinecone settings not found or invalid.");
        }

        var apiKey = pineconeSettings.ApiKey;
        _defaultIndexName = pineconeSettings.IndexName; // Use default index name
        var host = pineconeSettings.Host;

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(_defaultIndexName) || string.IsNullOrEmpty(host))
        {
            _logger.LogError("Pinecone configuration is missing or invalid. Check VectorStoreSettings:Pinecone:ApiKey, VectorStoreSettings:Pinecone:Environment, VectorStoreSettings:Pinecone:IndexName, and VectorStoreSettings:Pinecone:Host in settings.");
            throw new InvalidOperationException("Pinecone configuration is missing or invalid.");
        }
        _pineconeClient = new PineconeClient(apiKey);
    }

    public async Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
    {
        await UpsertAsync(embedding, metadata, _defaultIndexName, embedding.Count, cancellationToken);
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
            var vectorStoreSettings = _configProvider.GetSection<VectorStoreSettings>();
            var indexClient = _pineconeClient.Index(collectionName, vectorStoreSettings.Pinecone.Host); // Use provided collectionName

            // Check if index exists, create if not
            var indexes = await _pineconeClient.ListIndexesAsync(null, cancellationToken); // Fixed
            if (!indexes.Indexes!.Any(index => index.Name == collectionName))
            {
                _logger.LogInformation("Pinecone index {CollectionName} does not exist. Creating new index with dimension {EmbeddingDimension}.", collectionName, embeddingDimension);
                await _pineconeClient.CreateIndexAsync(
                    new CreateIndexRequest
                    {
                        Name = collectionName,
                        Dimension = embeddingDimension,
                        Metric = MetricType.Cosine,
                        Spec = new ServerlessIndexSpec
                        {
                            Serverless = new ServerlessSpec
                            {
                                Cloud = ServerlessSpecCloud.Azure,
                                Region = "eastus2",
                            }
                        },
                        DeletionProtection = DeletionProtection.Enabled
                    },
                    cancellationToken: cancellationToken);
                // Wait for index to be ready
                await Task.Delay(5000, cancellationToken); // Adjust delay as needed
            }

            var vectorId = Guid.NewGuid().ToString(); // Generate a unique ID for the vector

            var pineconeMetadata = metadata.Where(e => e.Value != null)
                                           .Select(e => new KeyValuePair<string, MetadataValue>(e.Key, new MetadataValue(e.Value)))
                                           .AsEnumerable();

            var vector = new Vector
            {
                Id = vectorId,
                Values = new ReadOnlyMemory<float>(embedding.Select(e => (float)e).ToArray()),
                Metadata = new Metadata(pineconeMetadata!)
            };

            var upsertRequest = new UpsertRequest { Vectors = [vector] };
            await indexClient.UpsertAsync(upsertRequest, null, cancellationToken);

            _logger.LogInformation("Successfully upserted vector {VectorId} to Pinecone index {CollectionName}. (Dimension: {EmbeddingDimension})", vectorId, collectionName, embeddingDimension);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting vector to Pinecone index {CollectionName}. (Dimension: {EmbeddingDimension})", collectionName, embeddingDimension);
            throw; // Re-throw to be handled by higher layers
        }
    }

    public async Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default)
    {
        return await QueryAsync(queryEmbedding, topK, metadataFilter, _defaultIndexName, cancellationToken);
    }

    public async Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, string collectionName, CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            throw new ArgumentException("Query embedding cannot be empty.");
        }

        try
        {
            var vectorStoreSettings = _configProvider.GetSection<VectorStoreSettings>();
            var indexClient = _pineconeClient.Index(collectionName, vectorStoreSettings.Pinecone.Host); // Use provided collectionName
            var pineconeFilter = metadataFilter != null ? new Metadata(metadataFilter.ToDictionary(k => k.Key, v => (MetadataValue?)v.Value)) : null;

            var queryRequest = new QueryRequest
            {
                Vector = new ReadOnlyMemory<float>(queryEmbedding.Select(e => (float)e).ToArray()),
                TopK = (uint)topK,
                IncludeValues = true,
                IncludeMetadata = true,
                Filter = pineconeFilter
            };

            var queryResponse = await indexClient.QueryAsync(queryRequest, cancellationToken: cancellationToken);

            var results = queryResponse.Matches?.Select(m => new VectorStoreQueryResult
            {
                Id = m.Id,
                Embedding = m.Values.HasValue ? m.Values.Value.ToArray().Select(e => (double)e).ToList() : new List<double>(),
                Score = m.Score ?? 0,
                Metadata = m.Metadata != null ? m.Metadata.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty) : new Dictionary<string, string>(),
                Content = m.Metadata?.FirstOrDefault(md => md.Key == "Content").Value?.ToString() ?? string.Empty
            }).ToList() ?? new List<VectorStoreQueryResult>();

            _logger.LogInformation("Successfully queried Pinecone index {CollectionName} with TopK {TopK}. Found {Count} matches.", collectionName, topK, results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying Pinecone index {CollectionName}.", collectionName);
            throw; // Re-throw to be handled by higher layers
        }
    }
}
