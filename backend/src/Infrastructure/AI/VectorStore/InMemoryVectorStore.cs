using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using System.Collections.Concurrent;

namespace backend.Infrastructure.AI.VectorStore
{
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentDictionary<string, TextChunk> _store = new ConcurrentDictionary<string, TextChunk>();

        public Task UpsertAsync(TextChunk chunk, CancellationToken cancellationToken = default)
        {
            _store[chunk.Id] = chunk;
            return Task.CompletedTask;
        }

        public Task<List<TextChunk>> QueryAsync(string queryText, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default)
        {
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
            }).Take(topK).ToList();

            return Task.FromResult(results);
        }
    }
}
