using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IVectorStore
{
    Task UpsertAsync(List<float> embedding, Dictionary<string, string> metadata, CancellationToken cancellationToken = default);
    Task UpsertAsync(List<float> embedding, Dictionary<string, string> metadata, string collectionName, int embeddingDimension, CancellationToken cancellationToken = default); // New overload
    Task<List<VectorStoreQueryResult>> QueryAsync(float[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, CancellationToken cancellationToken = default);
    Task<List<VectorStoreQueryResult>> QueryAsync(float[] queryEmbedding, int topK, Dictionary<string, string> metadataFilter, string collectionName, CancellationToken cancellationToken = default); // New overload
}
