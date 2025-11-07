using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Families.Specifications;

namespace backend.Application.Services;

public class FamilyTreeService(IApplicationDbContext context) : IFamilyTreeService
{
    private readonly IApplicationDbContext _context = context;

    public async Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default)
    {
        var familySpec = new FamilyByIdWithMembersAndRelationshipsSpec(familyId);
        var family = await _context.Families
            .WithSpecification(familySpec)
            .FirstOrDefaultAsync(cancellationToken);

        if (family == null) return; // Family not found

        family.RecalculateStats();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
