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

        // Get all relationships within the filtered families for detailed stats calculation
        var relationships = await _context.Relationships
            .WithSpecification(relationshipsInFamiliesSpec)
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

        var membersPerGeneration = CalculateMembersPerGeneration(members, relationships);

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
            AverageAge = averageAge,
            MembersPerGeneration = membersPerGeneration
        };

        return Result<DashboardStatsDto>.Success(stats);
    }

    private Dictionary<int, int> CalculateMembersPerGeneration(List<Domain.Entities.Member> members, List<Domain.Entities.Relationship> relationships)
    {
        var membersPerGeneration = new Dictionary<int, int>();
        if (!members.Any()) return membersPerGeneration;

        var graph = new Dictionary<Guid, List<Guid>>(); // Parent -> Children
        var parentsMap = new Dictionary<Guid, List<Guid>>(); // Child -> Parents

        foreach (var member in members)
        {
            graph[member.Id] = new List<Guid>();
            parentsMap[member.Id] = new List<Guid>();
        }

        foreach (var rel in relationships)
        {
            // Only consider parent-child relationships for generation calculation
            if (rel.Type == Domain.Enums.RelationshipType.Father || rel.Type == Domain.Enums.RelationshipType.Mother)
            {
                if (graph.ContainsKey(rel.SourceMemberId))
                {
                    graph[rel.SourceMemberId].Add(rel.TargetMemberId);
                }
                if (parentsMap.ContainsKey(rel.TargetMemberId))
                {
                    parentsMap[rel.TargetMemberId].Add(rel.SourceMemberId);
                }
            }
        }

        // Find root members (those with no parents within the current set of members/relationships)
        var rootMembers = members.Where(m => !parentsMap.ContainsKey(m.Id) || !parentsMap[m.Id].Any(pId => members.Any(mem => mem.Id == pId))).ToList();

        // Fallback if no explicit roots found but members exist
        if (!rootMembers.Any() && members.Any())
        {
            rootMembers = members.Where(m => !relationships.Any(r => r.TargetMemberId == m.Id && (r.Type == Domain.Enums.RelationshipType.Father || r.Type == Domain.Enums.RelationshipType.Mother))).ToList();
        }

        // Final fallback if still no roots, pick first member
        if (!rootMembers.Any() && members.Any())
        {
            rootMembers.Add(members.First());
        }

        var memberGenerations = new Dictionary<Guid, int>();
        var queue = new Queue<(Guid memberId, int generation)>();
        var visited = new HashSet<Guid>();

        foreach (var root in rootMembers)
        {
            if (!visited.Contains(root.Id))
            {
                queue.Enqueue((root.Id, 1));
                visited.Add(root.Id);
                memberGenerations[root.Id] = 1;

                while (queue.Any())
                {
                    var (currentMemberId, currentGeneration) = queue.Dequeue();

                    if (graph.ContainsKey(currentMemberId))
                    {
                        foreach (var childId in graph[currentMemberId])
                        {
                            if (!visited.Contains(childId))
                            {
                                visited.Add(childId);
                                memberGenerations[childId] = currentGeneration + 1;
                                queue.Enqueue((childId, currentGeneration + 1));
                            }
                            else
                            {
                                // If already visited, ensure we assign the lowest generation number
                                // This handles cases where a member might have parents from different "generation paths"
                                if (memberGenerations.ContainsKey(childId) && currentGeneration + 1 < memberGenerations[childId])
                                {
                                    memberGenerations[childId] = currentGeneration + 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Populate membersPerGeneration dictionary
        foreach (var entry in memberGenerations)
        {
            var generation = entry.Value;
            if (!membersPerGeneration.ContainsKey(generation))
            {
                membersPerGeneration[generation] = 0;
            }
            membersPerGeneration[generation]++;
        }

        return membersPerGeneration;
    }
}

