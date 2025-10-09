using backend.Application.Common.Models;

namespace backend.Application.VectorStore;

public interface IVectorStore
{
    Task<Result> UpsertDocumentsAsync(IEnumerable<VectorDocument> documents, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VectorDocument>>> QueryAsync(VectorQuery query, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(IEnumerable<string> documentIds, CancellationToken cancellationToken = default);
}
