using backend.Application.Common.Models;

namespace backend.Application.Common.Interfaces;

public interface IGlobalSearchService
{
    Task UpsertEntityAsync<T>(T entity, string entityType, Func<T, string> textExtractor, Func<T, Dictionary<string, string>> metadataExtractor, CancellationToken cancellationToken = default);

    Task<Result<List<GlobalSearchResult>>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task DeleteEntityFromSearchAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
}
