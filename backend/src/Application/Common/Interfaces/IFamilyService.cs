using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IFamilyService : IBaseCrudService<Family, FamilyDto>
{
    Task<Result<PaginatedList<FamilyDto>>> SearchAsync(FamilyFilterModel filter);
}