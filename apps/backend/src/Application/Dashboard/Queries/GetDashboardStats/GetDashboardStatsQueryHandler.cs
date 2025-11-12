using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Dashboard.Specifications;

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser user) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {

        IEnumerable<Guid>? accessibleFamilyIds = null;
        if (!_authorizationService.IsAdmin())
        {
            // Lọc các gia đình mà người dùng có quyền truy cập nếu không phải là admin
            var familyUsersSpec = new FamilyUsersByUserIdSpec(_user.UserId);
            accessibleFamilyIds = await _context.FamilyUsers
                .WithSpecification(familyUsersSpec)
                .Select(fu => fu.FamilyId)
                .ToListAsync(cancellationToken);
        }

        // Áp dụng Specification để lọc các gia đình
        var familiesSpec = new FamiliesCountSpec(accessibleFamilyIds, request.FamilyId);
        var filteredFamiliesQuery = _context.Families.WithSpecification(familiesSpec);

        var totalFamilies = await filteredFamiliesQuery.CountAsync(cancellationToken);

        // Áp dụng Specification để lọc thành viên trong các gia đình đã lọc
        var membersInFamiliesSpec = new MembersInFamiliesSpec(filteredFamiliesQuery);
        var totalMembers = await _context.Members.WithSpecification(membersInFamiliesSpec).CountAsync(cancellationToken);

        // Áp dụng Specification để lọc mối quan hệ trong các gia đình đã lọc
        var relationshipsInFamiliesSpec = new RelationshipsInFamiliesSpec(filteredFamiliesQuery);
        var totalRelationships = await _context.Relationships.WithSpecification(relationshipsInFamiliesSpec).CountAsync(cancellationToken);

        var totalGenerations = await filteredFamiliesQuery.SumAsync(e => e.TotalGenerations, cancellationToken);

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

