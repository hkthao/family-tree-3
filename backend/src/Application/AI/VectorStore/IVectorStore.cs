using backend.Application.Common.Models;

namespace backend.Application.AI.VectorStore
{
    public interface IVectorStore
    {
        Task<Result> UpsertDocumentsAsync(IEnumerable<VectorDocument> documents, CancellationToken cancellationToken = default);
        Task<Result> UpsertVectorAsync(string id, float[] vector, Dictionary<string, string> metadata, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<VectorDocument>>> QueryAsync(VectorQuery query, CancellationToken cancellationToken = default);
        Task<Result<List<string>>> QueryNearestVectorsAsync(float[] vector, int topK, CancellationToken cancellationToken = default);
        Task<Result> DeleteAsync(IEnumerable<string> documentIds, CancellationToken cancellationToken = default);
    }
}
