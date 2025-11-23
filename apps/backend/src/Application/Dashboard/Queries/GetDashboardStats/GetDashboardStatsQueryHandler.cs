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

        // Get all members within the filtered families for detailed stats calculation
        var members = await _context.Members
            .WithSpecification(membersInFamiliesSpec)
            .ToListAsync(cancellationToken);

        // Calculate Living/Deceased Members
        var livingMembersCount = members.Count(m => !m.IsDeceased);
        var deceasedMembersCount = members.Count(m => m.IsDeceased);

        // Calculate Gender Ratio
        var totalMembersForGender = members.Count(m => !string.IsNullOrEmpty(m.Gender));
        var maleCount = members.Count(m => m.Gender == backend.Domain.Enums.Gender.Male.ToString());
        var femaleCount = members.Count(m => m.Gender == backend.Domain.Enums.Gender.Female.ToString());
        var maleRatio = totalMembersForGender > 0 ? (double)maleCount / totalMembersForGender : 0.0;
        var femaleRatio = totalMembersForGender > 0 ? (double)femaleCount / totalMembersForGender : 0.0;

        // Calculate Average Age
        var membersWithKnownBirthDate = members.Where(m => m.DateOfBirth.HasValue).ToList();
        double averageAge = 0.0;

        if (membersWithKnownBirthDate.Any())
        {
            var totalAge = membersWithKnownBirthDate.Sum(m =>
            {
                var yearOfBirth = m.DateOfBirth!.Value.Year;
                if (m.IsDeceased && m.DateOfDeath.HasValue)
                {
                    return m.DateOfDeath.Value.Year - yearOfBirth;
                }
                // For living or deceased without DateOfDeath, use current year
                return DateTime.Now.Year - yearOfBirth;
            });
            averageAge = (double)totalAge / membersWithKnownBirthDate.Count;
        }

        var stats = new DashboardStatsDto
        {
            TotalFamilies = totalFamilies,
            TotalMembers = totalMembers,
            TotalRelationships = totalRelationships,
            TotalGenerations = totalGenerations,
            MaleRatio = maleRatio,
            FemaleRatio = femaleRatio,
            LivingMembersCount = livingMembersCount,
            DeceasedMembersCount = deceasedMembersCount,
            AverageAge = averageAge
        };

        return Result<DashboardStatsDto>.Success(stats);
    }
}

