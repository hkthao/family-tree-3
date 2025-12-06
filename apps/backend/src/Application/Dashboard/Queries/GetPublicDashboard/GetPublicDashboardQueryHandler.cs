using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Dashboard.Queries.GetPublicDashboard;

public class GetPublicDashboardQueryHandler : IRequestHandler<GetPublicDashboardQuery, Result<PublicDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public GetPublicDashboardQueryHandler(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Result<PublicDashboardDto>> Handle(GetPublicDashboardQuery request, CancellationToken cancellationToken)
    {
        var allPublicFamilies = await _context.Families
            .Where(f => f.Id == request.FamilyId && f.Visibility == "Public" && !f.IsDeleted)
            .Include(f => f.Members.Where(m => !m.IsDeleted))
            .Include(f => f.Relationships.Where(r => !r.IsDeleted))
            .Include(f => f.Events.Where(e => !e.IsDeleted))
            .ToListAsync(cancellationToken);

        var dashboardDto = new PublicDashboardDto();

        dashboardDto.TotalPublicFamilies = allPublicFamilies.Count;

        var allPublicMembers = allPublicFamilies.SelectMany(f => f.Members).Where(m => !m.IsDeleted).ToList();
        var allPublicRelationships = allPublicFamilies.SelectMany(f => f.Relationships).Where(r => !r.IsDeleted).ToList();
        var allPublicEventsFromFamilies = allPublicFamilies.SelectMany(f => f.Events).Where(e => !e.IsDeleted).ToList();

        dashboardDto.TotalPublicEvents = allPublicEventsFromFamilies.Count;

        dashboardDto.TotalPublicMembers = allPublicMembers.Count;
        dashboardDto.TotalPublicRelationships = allPublicRelationships.Count;

        // Gender Ratios
        var maleMembers = allPublicMembers.Count(m => m.Gender == Gender.Male.ToString());
        var femaleMembers = allPublicMembers.Count(m => m.Gender == Gender.Female.ToString());
        var totalGenderedMembers = maleMembers + femaleMembers;

        if (totalGenderedMembers > 0)
        {
            dashboardDto.PublicMaleRatio = (int)Math.Round((double)maleMembers / totalGenderedMembers * 100);
            dashboardDto.PublicFemaleRatio = (int)Math.Round((double)femaleMembers / totalGenderedMembers * 100);
        }

        // Living and Deceased Members
        dashboardDto.PublicLivingMembersCount = allPublicMembers.Count(m => !m.IsDeceased);
        dashboardDto.PublicDeceasedMembersCount = allPublicMembers.Count(m => m.IsDeceased);

        // Average Age
        var livingMembersWithBirthDate = allPublicMembers
            .Where(m => !m.IsDeceased && m.DateOfBirth.HasValue)
            .ToList();

        if (livingMembersWithBirthDate.Any())
        {
            var totalAgeInYears = livingMembersWithBirthDate
                .Sum(m => (_dateTime.Now.Year - m.DateOfBirth!.Value.Year) - (m.DateOfBirth.Value.Date > _dateTime.Now.AddYears(-(_dateTime.Now.Year - m.DateOfBirth.Value.Year)).Date ? 1 : 0));
            dashboardDto.PublicAverageAge = (int)Math.Round((double)totalAgeInYears / livingMembersWithBirthDate.Count);
        }

        // Generations and Members Per Generation
        int maxGlobalGenerations = 0;
        var globalMembersPerGeneration = new Dictionary<int, int>();

        foreach (var family in allPublicFamilies)
        {
            var filteredFamilyMembers = family.Members.Where(m => !m.IsDeleted).ToList();
            var filteredFamilyRelationships = family.Relationships.Where(r => !r.IsDeleted).ToList();

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
        dashboardDto.TotalPublicGenerations = maxGlobalGenerations;
        dashboardDto.PublicMembersPerGeneration = globalMembersPerGeneration;

        return Result<PublicDashboardDto>.Success(dashboardDto);
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

        // Only add entries to graph and childrenGraph if there's a relevant relationship
        var relevantRelationships = relationships
            .Where(r => !r.IsDeleted &&
                        members.Any(m => m.Id == r.SourceMemberId) &&
                        members.Any(m => m.Id == r.TargetMemberId) &&
                        (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))
            .ToList();

        foreach (var rel in relevantRelationships)
        {
            // Ensure target (child) has an entry in graph
            if (!graph.ContainsKey(rel.TargetMemberId))
            {
                graph[rel.TargetMemberId] = new List<Guid>();
            }
            if (!graph[rel.TargetMemberId].Contains(rel.SourceMemberId))
            {
                graph[rel.TargetMemberId].Add(rel.SourceMemberId);
            }

            // Ensure source (parent) has an entry in childrenGraph
            if (!childrenGraph.ContainsKey(rel.SourceMemberId))
            {
                childrenGraph[rel.SourceMemberId] = new List<Guid>();
            }
            if (!childrenGraph[rel.SourceMemberId].Contains(rel.TargetMemberId))
            {
                childrenGraph[rel.SourceMemberId].Add(rel.TargetMemberId);
            }
        }

        // Identify potential root members: those in 'members' that are not children (TargetMemberId)
        // in any of the relevant relationships.
        var allChildrenInRelevantRelationships = relevantRelationships.Select(r => r.TargetMemberId).ToHashSet();
        var rootMembers = members.Where(m => !allChildrenInRelevantRelationships.Contains(m.Id)).ToList();

        // Handle cases where there are no relationships defined, or all members are isolated.
        // In such scenarios, all members are considered generation 1.
        if (!rootMembers.Any() && members.Any())
        {
            foreach (var member in members)
            {
                memberGenerations[member.Id] = 1;
            }
        }
        else
        {
            var queue = new Queue<Guid>();
            foreach (var root in rootMembers)
            {
                memberGenerations[root.Id] = 1;
                queue.Enqueue(root.Id);
            }

            while (queue.Any())
            {
                var currentMemberId = queue.Dequeue();
                var currentGeneration = memberGenerations[currentMemberId];

                if (childrenGraph.TryGetValue(currentMemberId, out var children))
                {
                    foreach (var childId in children)
                    {
                        if (members.Any(m => m.Id == childId)) // Ensure child is part of the current family members collection
                        {
                            if (!memberGenerations.ContainsKey(childId) || memberGenerations[childId] < currentGeneration + 1)
                            {
                                memberGenerations[childId] = currentGeneration + 1;
                                queue.Enqueue(childId);
                            }
                        }
                    }
                }
            }
        }
        
        // Handle any members that were not reached by the BFS (e.g., truly isolated members, or members
        // whose parents are outside the 'members' collection and thus not included in 'relevantRelationships').
        // These should also be considered Generation 1 as they effectively start new branches.
        foreach (var member in members)
        {
            if (!memberGenerations.ContainsKey(member.Id))
            {
                memberGenerations[member.Id] = 1;
            }
        }


        int maxGen = memberGenerations.Any() ? memberGenerations.Values.Max() : 0;
        var membersPerGen = memberGenerations.Values
            .GroupBy(gen => gen)
            .ToDictionary(g => g.Key, g => g.Count());

        return (maxGen, membersPerGen);
    }
}
