using backend.Application.Common.Models;
using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IFamilyService
{
    Task<List<Family>> GetAllFamiliesAsync();
    Task<Family?> GetFamilyByIdAsync(Guid id);
    Task<List<Family>> GetFamiliesByIdsAsync(IEnumerable<Guid> ids);
    Task<Family> CreateFamilyAsync(Family family);
    Task UpdateFamilyAsync(Family family);
    Task DeleteFamilyAsync(Guid id);
}