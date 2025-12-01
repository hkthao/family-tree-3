using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using backend.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
            .Where(f => f.Visibility == "Public" && !f.IsDeleted)
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
            dashboardDto.PublicMaleRatio = (double)maleMembers / totalGenderedMembers;
            dashboardDto.PublicFemaleRatio = (double)femaleMembers / totalGenderedMembers;
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
            dashboardDto.PublicAverageAge = (double)totalAgeInYears / livingMembersWithBirthDate.Count;
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