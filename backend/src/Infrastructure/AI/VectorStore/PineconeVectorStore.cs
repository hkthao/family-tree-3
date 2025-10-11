using backend.Application.Common.Models;
using backend.Application.AI.VectorStore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pinecone;

namespace backend.Infrastructure.AI.VectorStore
{
    public class PineconeVectorStore : IVectorStore
    {
        private readonly ILogger<PineconeVectorStore> _logger;
        private readonly IOptions<VectorStoreSettings> _vectorStoreSettings;
        private readonly PineconeClient _pineconeClient;
        private readonly IndexClient _index;
        private readonly string _indexName;

        public PineconeVectorStore(ILogger<PineconeVectorStore> logger, IOptions<VectorStoreSettings> vectorStoreSettings)
        {
            _logger = logger;
            _vectorStoreSettings = vectorStoreSettings;

            var pineconeSettings = _vectorStoreSettings.Value.Pinecone;

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

        public async Task<Result> UpsertDocumentsAsync(IEnumerable<VectorDocument> documents, CancellationToken cancellationToken = default)
        {
            try
            {
                var index = _index;

                var vectors = documents.Select(d => new Vector
                {
                    Id = d.Id,
                    Values = new ReadOnlyMemory<float>([.. d.Vector]),
                    Metadata = d.Metadata != null ? new Metadata(d.Metadata.ToDictionary(k => k.Key, v => (MetadataValue?)v.Value)) : null
                }).ToList();

                var upsertRequest = new UpsertRequest { Vectors = vectors };
                await index.UpsertAsync(upsertRequest, null, cancellationToken);

                _logger.LogInformation("Successfully upserted {Count} documents to Pinecone index {IndexName}.", documents.Count(), _indexName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting documents to Pinecone index {IndexName}.", _indexName);
                return Result.Failure(ex.Message, "PineconeError");
            }
        }

        public async Task<Result> UpsertVectorAsync(string id, float[] vector, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            try
            {
                var index = _index;

                var pineconeVector = new Vector
                {
                    Id = id,
                    Values = new ReadOnlyMemory<float>([.. vector]),
                    Metadata = metadata != null ? new Metadata(metadata.ToDictionary(k => k.Key, v => (MetadataValue?)v.Value)) : null
                };

                var upsertRequest = new UpsertRequest { Vectors = [pineconeVector] };
                await index.UpsertAsync(upsertRequest, null, cancellationToken);

                _logger.LogInformation("Successfully upserted vector with ID {Id} to Pinecone index {IndexName}.", id, _indexName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting vector with ID {Id} to Pinecone index {IndexName}.", id, _indexName);
                return Result.Failure(ex.Message, "PineconeError");
            }
        }

        public async Task<Result<IEnumerable<VectorDocument>>> QueryAsync(VectorQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                var index = _index;

                var queryRequest = new QueryRequest
                {
                    Vector = new ReadOnlyMemory<float>([.. query.Vector]),
                    TopK = (uint)query.TopK,
                    IncludeValues = true,
                    IncludeMetadata = true,
                    Filter = query.Filter != null ? new Metadata(query.Filter.ToDictionary(k => k.Key, v => (MetadataValue?)v.Value)) : null
                };

                var queryResponse = await index.QueryAsync(queryRequest, cancellationToken: cancellationToken);

                var results = queryResponse.Matches?.Select(m => new VectorDocument
                {
                    Id = m.Id,
                    Vector = m.Values.HasValue ? m.Values.Value.ToArray() : [],
                    Metadata = m.Metadata != null ? m.Metadata.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty) : null
                });

                _logger.LogInformation("Successfully queried Pinecone index {IndexName} with TopK {TopK}. Found {Count} matches.", _indexName, query.TopK, results?.Count() ?? 0);
                return Result<IEnumerable<VectorDocument>>.Success(results ?? []);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Pinecone index {IndexName}.", _indexName);
                return Result<IEnumerable<VectorDocument>>.Failure(ex.Message, "PineconeError");
            }
        }

        public async Task<Result<List<string>>> QueryNearestVectorsAsync(float[] vector, int topK, CancellationToken cancellationToken = default)
        {
            try
            {
                var index = _index;

                var queryRequest = new QueryRequest
                {
                    Vector = new ReadOnlyMemory<float>([.. vector]),
                    TopK = (uint)topK,
                    IncludeValues = false, // We only need IDs for this method
                    IncludeMetadata = false
                };

                var queryResponse = await index.QueryAsync(queryRequest, cancellationToken: cancellationToken);

                var results = queryResponse.Matches?.Select(m => m.Id).ToList();

                _logger.LogInformation("Successfully queried Pinecone index {IndexName} for nearest vectors with TopK {TopK}. Found {Count} matches.", _indexName, topK, results?.Count ?? 0);
                return Result<List<string>>.Success(results ?? []);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying nearest vectors from Pinecone index {IndexName}.", _indexName);
                return Result<List<string>>.Failure(ex.Message, "PineconeError");
            }
        }

        public async Task<Result> DeleteAsync(IEnumerable<string> documentIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var index = _index;

                var deleteRequest = new DeleteRequest { Ids = [.. documentIds], DeleteAll = false };
                await index.DeleteAsync(deleteRequest, null, cancellationToken);

                _logger.LogInformation("Successfully deleted {Count} documents from Pinecone index {IndexName}.", documentIds.Count(), _indexName);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting documents from Pinecone index {IndexName}.", _indexName);
                return Result.Failure(ex.Message, "PineconeError");
            }
        }
    }
}