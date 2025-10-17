namespace backend.Application.Common.Interfaces;

public interface IFamilyTreeService
{
    Task<int> CalculateTotalMembers(Guid familyId, CancellationToken cancellationToken = default);
    Task<int> CalculateTotalGenerations(Guid familyId, CancellationToken cancellationToken = default);
    Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default);
}
