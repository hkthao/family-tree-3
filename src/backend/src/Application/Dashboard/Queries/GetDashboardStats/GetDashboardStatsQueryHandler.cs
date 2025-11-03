using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser user) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {

        IQueryable<Domain.Entities.Family> familiesQuery = _context.Families;
        if (!_authorizationService.IsAdmin())
        {
            // Filter families by user access if not admin
            var accessibleFamilyIds = await _context.FamilyUsers
                .Where(fu => fu.UserId == _user.UserId)
                .Select(fu => fu.FamilyId)
                .ToListAsync(cancellationToken);

            familiesQuery = familiesQuery.Where(f => accessibleFamilyIds.Contains(f.Id));
        }

        if (request.FamilyId.HasValue)
        {
            familiesQuery = familiesQuery.Where(f => f.Id == request.FamilyId.Value);
        }

        var totalFamilies = await familiesQuery.CountAsync(cancellationToken);
        var totalMembers = await _context.Members.Where(m => familiesQuery.Select(f => f.Id).Contains(m.FamilyId)).CountAsync(cancellationToken);
        var totalRelationships = await _context.Relationships.Where(r => familiesQuery.Select(f => f.Id).Contains(r.SourceMember.FamilyId)).CountAsync(cancellationToken);

        var totalGenerations = await familiesQuery.SumAsync(e=>e.TotalGenerations, cancellationToken);

        var stats = new DashboardStatsDto
        {
            TotalFamilies = totalFamilies,
            TotalMembers = totalMembers,
            TotalRelationships = totalRelationships,
            TotalGenerations = totalGenerations
        };

        return Result<DashboardStatsDto>.Success(stats);
    }
}

