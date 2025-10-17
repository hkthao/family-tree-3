using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IVectorStore
{
    Task UpsertAsync(TextChunk chunk, CancellationToken cancellationToken = default);
    Task<List<TextChunk>> QueryAsync(float[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default);
}
