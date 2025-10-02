using backend.Application.Common.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Families;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;

    public FamilyService(IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<List<Family>> GetAllFamiliesAsync()
    {
        var families = await _familyRepository.GetAllAsync();
        return families.ToList();
    }

    public async Task<Family?> GetFamilyByIdAsync(Guid id)
    {
        return await _familyRepository.GetByIdAsync(id);
    }

    public async Task<List<Family>> GetFamiliesByIdsAsync(IEnumerable<Guid> ids)
    {
        var families = await _familyRepository.GetByIdsAsync(ids);
        return families.ToList();
    }

    public async Task<Family> CreateFamilyAsync(Family family)
    {
        return await _familyRepository.AddAsync(family);
    }

    public async Task UpdateFamilyAsync(Family family)
    {
        await _familyRepository.UpdateAsync(family);
    }

    public async Task DeleteFamilyAsync(Guid id)
    {
        var family = await _familyRepository.GetByIdAsync(id);
        if (family != null)
        {
            await _familyRepository.DeleteAsync(family);
        }
    }
}