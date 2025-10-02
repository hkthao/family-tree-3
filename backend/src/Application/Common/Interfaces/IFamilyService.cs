using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IFamilyService : IBaseCrudService<Family>
{
    Task<Result<List<Family>>> GetFamiliesByIdsAsync(IEnumerable<Guid> ids);
    Task<Result<PaginatedList<Family>>> SearchFamiliesAsync(string? keyword, int page, int itemsPerPage);
}