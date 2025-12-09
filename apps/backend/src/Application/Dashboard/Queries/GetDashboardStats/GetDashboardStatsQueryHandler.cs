using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Dashboard.Specifications;
using backend.Application.Dashboard;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser user, IDateTime dateTime) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IDateTime _dateTime = dateTime;

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = new DashboardStatsDto();

        IEnumerable<Guid>? accessibleFamilyIds = null;
        if (!_authorizationService.IsAdmin())
        {
            var familyUsersSpec = new FamilyUsersByUserIdSpec(_user.UserId);
            accessibleFamilyIds = await _context.FamilyUsers
                .WithSpecification(familyUsersSpec)
                .Select(fu => fu.FamilyId)
                .ToListAsync(cancellationToken);
        }

        var familiesSpec = new FamiliesCountSpec(accessibleFamilyIds, request.FamilyId);
        var filteredFamiliesQuery = _context.Families.WithSpecification(familiesSpec);

        // Fetch members, relationships, and events based on the filtered families query
        var members = await _context.Members
            .Where(m => filteredFamiliesQuery.Any(f => f.Id == m.FamilyId))
            .Where(m => !m.IsDeleted)
            .ToListAsync(cancellationToken);

        var relationships = await _context.Relationships
            .Where(r => filteredFamiliesQuery.Any(f => f.Id == r.FamilyId))
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);
        
        var events = await _context.Events
            .Where(e => filteredFamiliesQuery.Any(f => f.Id == e.FamilyId))
            .Where(e => !e.IsDeleted)
            .ToListAsync(cancellationToken);
        
        stats.TotalFamilies = await filteredFamiliesQuery.CountAsync(cancellationToken);
        stats.TotalMembers = members.Count();
        stats.TotalRelationships = relationships.Count();
        stats.TotalEvents = events.Count();

        // Calculate Living/Deceased Members
        stats.LivingMembersCount = members.Count(m => !m.IsDeceased);
        stats.DeceasedMembersCount = members.Count(m => m.IsDeceased);

        // Calculate Gender Ratio
        var totalMembersForGender = members.Count(m => !string.IsNullOrEmpty(m.Gender));
        var maleCount = members.Count(m => m.Gender == Gender.Male.ToString());
        var femaleCount = members.Count(m => m.Gender == Gender.Female.ToString());
        stats.MaleRatio = totalMembersForGender > 0 ? (double)maleCount / totalMembersForGender : 0.0;
        stats.FemaleRatio = totalMembersForGender > 0 ? (double)femaleCount / totalMembersForGender : 0.0;

        // Calculate Average Age
        var livingMembersWithBirthDate = members
            .Where(m => !m.IsDeceased && m.DateOfBirth.HasValue)
            .ToList();

        if (livingMembersWithBirthDate.Any())
        {
            var totalAgeInYears = livingMembersWithBirthDate
                .Sum(m => (_dateTime.Now.Year - m.DateOfBirth!.Value.Year) - (m.DateOfBirth.Value.Date > _dateTime.Now.AddYears(-(_dateTime.Now.Year - m.DateOfBirth.Value.Year)).Date ? 1 : 0));
            stats.AverageAge = (int)Math.Round((double)totalAgeInYears / livingMembersWithBirthDate.Count);
        }

        // Generations and Members Per Generation
        int maxGlobalGenerations = 0;
        var globalMembersPerGeneration = new Dictionary<int, int>();

        var familiesInScope = await filteredFamiliesQuery.ToListAsync(cancellationToken); 

        foreach (var family in familiesInScope)
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
        stats.TotalGenerations = maxGlobalGenerations;
        stats.MembersPerGeneration = globalMembersPerGeneration;

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