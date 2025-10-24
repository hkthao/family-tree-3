using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IGlobalSearchService
{
    Task UpsertFamilyForSearchAsync(Family family, CancellationToken cancellationToken = default);
    // TODO: Add methods for other entity types (members, relationships, documentation)

    Task<Result<List<GlobalSearchResult>>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task DeleteEntityFromSearchAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
}
