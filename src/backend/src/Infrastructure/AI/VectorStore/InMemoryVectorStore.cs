using System.Collections.Concurrent;
using backend.Application.AI.VectorStore;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Infrastructure.AI.VectorStore
{
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentDictionary<string, TextChunk> _store = new();

        public Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            return UpsertAsync(embedding, metadata, "default_collection", embedding.Count, cancellationToken);
        }

        public Task UpsertAsync(List<double> embedding, Dictionary<string, string> metadata, string collectionName, int embeddingDimension, CancellationToken cancellationToken = default)
        {
            var chunk = new TextChunk
            {
                Id = Guid.NewGuid().ToString(), // Generate a unique ID
                Embedding = [.. embedding],
                Metadata = metadata
            };
            _store[chunk.Id] = chunk;
            return Task.CompletedTask;
        }

        public Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default)
        {
            return QueryAsync(queryEmbedding, topK, metadataFilter, "default_collection", cancellationToken);
        }

        public Task<List<VectorStoreQueryResult>> QueryAsync(double[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, string collectionName, CancellationToken cancellationToken = default)
        {
            if (queryEmbedding == null || queryEmbedding.Length == 0)
            {
                throw new ArgumentException("Query embedding cannot be empty.");
            }

            // This is a dummy implementation for in-memory store.
            // In a real scenario, this would involve vector similarity search.
            var results = _store.Values.Where(chunk =>
            {
                bool matches = true;
                foreach (var filter in metadataFilter)
                {
                    if (!chunk.Metadata.TryGetValue(filter.Key, out var value) || value != filter.Value)
                    {
                        matches = false;
                        break;
                    }
                }
                return matches;
            }).Select(chunk => new VectorStoreQueryResult
            {
                Id = chunk.Id,
                Metadata = chunk.Metadata,
                Embedding = chunk.Embedding?.ToList() ?? [],
                Score = 1.0f, // Dummy score for in-memory store
                Content = chunk.Metadata.GetValueOrDefault("Content", string.Empty)
            }).Take(topK).ToList();

            return Task.FromResult(results);
        }

        public Task DeleteAsync(string entityId, string collectionName, CancellationToken cancellationToken = default)
        {
            // In-memory store doesn't use collectionName for deletion in this simple implementation
            _store.TryRemove(entityId, out _);
            return Task.CompletedTask;
        }
    }
}
