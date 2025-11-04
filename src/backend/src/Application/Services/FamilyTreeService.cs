using backend.Application.Common.Interfaces;

namespace backend.Application.Services;

public class FamilyTreeService(IApplicationDbContext context) : IFamilyTreeService
{
    private readonly IApplicationDbContext _context = context;

    public async Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default)
    {
        var family = await _context.Families
            .Include(f => f.Members)
            .Include(f => f.Relationships)
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null) return; // Family not found

        family.RecalculateStats();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
