using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Dashboard.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser user, IDateTime dateTime) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IDateTime _dateTime = dateTime;

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
        var members = await _context.Members.WithSpecification(membersInFamiliesSpec).ToListAsync(cancellationToken);
        var totalMembers = members.Count;


        // Áp dụng Specification để lọc mối quan hệ trong các gia đình đã lọc
        var relationshipsInFamiliesSpec = new RelationshipsInFamiliesSpec(filteredFamiliesQuery);
        var relationships = await _context.Relationships.WithSpecification(relationshipsInFamiliesSpec).ToListAsync(cancellationToken);
        var totalRelationships = relationships.Count;

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
        var livingMembersWithBirthDate = members
            .Where(m => !m.IsDeceased && m.DateOfBirth.HasValue)
            .ToList();
        int averageAge = 0;

        if (livingMembersWithBirthDate.Any())
        {
            var totalAgeInYears = livingMembersWithBirthDate
                .Sum(m => (_dateTime.Now.Year - m.DateOfBirth!.Value.Year) - (m.DateOfBirth.Value.Date > _dateTime.Now.AddYears(-(_dateTime.Now.Year - m.DateOfBirth.Value.Year)).Date ? 1 : 0));
            averageAge = (int)Math.Round((double)totalAgeInYears / livingMembersWithBirthDate.Count);
        }

        // Generations and Members Per Generation
        int maxGlobalGenerations = 0;
        var globalMembersPerGeneration = new Dictionary<int, int>();

        var families = await filteredFamiliesQuery.ToListAsync(cancellationToken); // Re-fetch families to iterate

        foreach (var family in families)
        {
            var filteredFamilyMembers = members.Where(m => m.FamilyId == family.Id).ToList();
            var filteredFamilyRelationships = relationships.Where(r => r.FamilyId == family.Id).ToList();

            if (!filteredFamilyMembers.Any()) continue;

            var (familyMaxGenerations, familyMembersPerGeneration) = CalculateGenerations(filteredFamilyMembers, filteredFamilyRelationships);

            if (familyMaxGenerations > maxGlobalGenerations)
            {
                maxGlobalGenerations = familyMaxGenerations;
            }

            foreach (var entry in familyMembersPerGeneration)
            {
                if (globalMembersPerGeneration.ContainsKey(entry.Key))
                {
                    globalMembersPerGeneration[entry.Key] += entry.Value;
                }
                else
                {
                    globalMembersPerGeneration[entry.Key] = entry.Value;
                }
            }
        }

        var stats = new DashboardStatsDto
        {
            TotalFamilies = totalFamilies,
            TotalMembers = totalMembers,
            TotalRelationships = totalRelationships,

            MaleRatio = maleRatio,
            FemaleRatio = femaleRatio,
            LivingMembersCount = livingMembersCount,
            DeceasedMembersCount = deceasedMembersCount,
            AverageAge = averageAge,
            MembersPerGeneration = globalMembersPerGeneration
        };

        return Result<DashboardStatsDto>.Success(stats);
    }

    private (int maxGenerations, Dictionary<int, int> membersPerGeneration) CalculateGenerations(
        ICollection<Member> members,
        ICollection<Relationship> relationships)
    {
        if (!members.Any())
        {
            return (0, new Dictionary<int, int>());
        }

        var memberGenerations = new Dictionary<Guid, int>();
        var graph = new Dictionary<Guid, List<Guid>>(); // child -> parents
        var childrenGraph = new Dictionary<Guid, List<Guid>>(); // parent -> children

        foreach (var member in members)
        {
            graph[member.Id] = new List<Guid>();
            childrenGraph[member.Id] = new List<Guid>();
        }

        foreach (var rel in relationships.Where(r => !r.IsDeleted))
        {
            // Relationship type Father or Mother means SourceMember is a parent of TargetMember
            if (rel.Type == RelationshipType.Father || rel.Type == RelationshipType.Mother)
            {
                if (graph.ContainsKey(rel.TargetMemberId) && members.Any(m => m.Id == rel.SourceMemberId))
                {
                    graph[rel.TargetMemberId].Add(rel.SourceMemberId);
                }
                if (childrenGraph.ContainsKey(rel.SourceMemberId) && members.Any(m => m.Id == rel.TargetMemberId))
                {
                    childrenGraph[rel.SourceMemberId].Add(rel.TargetMemberId);
                }
            }
        }

        // Identify root members (those with no known parents within this family)
        var rootMembers = members.Where(m => !graph.ContainsKey(m.Id) || !graph[m.Id].Any()).ToList();

        // If there are no explicit roots, or all members seem to have parents,
        // it implies a disconnected graph or a single-generation family.
        // In such cases, assign generation 1 to all, or handle as appropriate for your definition of 'generation'.
        if (!rootMembers.Any() && members.Any())
        {
            foreach (var member in members)
            {
                memberGenerations[member.Id] = 1; // Assume all are generation 1 if no clear roots
            }
        }
        else
        {
            // Use a queue for BFS to determine generations
            var queue = new Queue<Guid>();
            foreach (var root in rootMembers)
            {
                memberGenerations[root.Id] = 1; // Roots are generation 1
                queue.Enqueue(root.Id);
            }

            while (queue.Any())
            {
                var currentMemberId = queue.Dequeue();
                var currentGeneration = memberGenerations[currentMemberId];

                // Find children of current member
                if (childrenGraph.TryGetValue(currentMemberId, out var children))
                {
                    foreach (var childId in children)
                    {
                        // If child's generation hasn't been set or can be set to a higher generation
                        if (!memberGenerations.ContainsKey(childId) || memberGenerations[childId] < currentGeneration + 1)
                        {
                            memberGenerations[childId] = currentGeneration + 1;
                            queue.Enqueue(childId);
                        }
                    }
                }
            }
        }


        int maxGen = memberGenerations.Any() ? memberGenerations.Values.Max() : 0;
        var membersPerGen = memberGenerations.Values
            .GroupBy(gen => gen)
            .ToDictionary(g => g.Key, g => g.Count());

        return (maxGen, membersPerGen);
    }
}

