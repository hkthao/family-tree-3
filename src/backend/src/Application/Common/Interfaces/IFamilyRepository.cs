using backend.Domain.Entities;

namespace backend.Application.Common.Interfaces;

public interface IFamilyRepository
{
    Task<Family?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Family family);
    void Update(Family family);
    void Delete(Family family);
}
