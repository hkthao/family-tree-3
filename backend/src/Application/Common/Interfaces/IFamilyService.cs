using backend.Application.Common.Models;
using backend.Application.Families.Queries;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IFamilyService : IBaseCrudService<Family, FamilyDto>
{
    Task<Result<List<FamilyDto>>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<Result<PaginatedList<FamilyDto>>> SearchAsync(FamilyFilterModel filter);
}