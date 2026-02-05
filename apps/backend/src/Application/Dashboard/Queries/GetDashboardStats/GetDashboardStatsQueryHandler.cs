using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using backend.Application.Common.Interfaces.Core;
using backend.Application.Common.Models;
using backend.Application.Dashboard.Specifications;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IApplicationDbContext context, IAuthorizationService authorizationService, ICurrentUser user, IDateTime dateTime, IMediator mediator) : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ICurrentUser _user = user;
    private readonly IDateTime _dateTime = dateTime;
    private readonly IMediator _mediator = mediator;

    // Định nghĩa một record lồng để chứa dữ liệu đã lọc
    private record FilteredDashboardData(
        IQueryable<Family> FilteredFamiliesQuery,
        IEnumerable<Member> Members,
        IEnumerable<Relationship> Relationships,
        IEnumerable<Event> Events,
        IEnumerable<Family> FamiliesInScope
    );

    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = new DashboardStatsDto();

        // Trích xuất logic lọc và lấy dữ liệu vào phương thức riêng tư
        var data = await _GetFilteredDashboardData(request, cancellationToken);

        // Sử dụng dữ liệu đã lọc để tính toán thống kê
        stats.TotalFamilies = await data.FilteredFamiliesQuery.CountAsync(cancellationToken);
        stats.TotalMembers = data.Members.Count();
        stats.TotalRelationships = data.Relationships.Count();
        stats.TotalEvents = data.Events.Count();

        // Calculate UpcomingEventsCount
        var today = _dateTime.Now.Date;
        var threeDaysLater = today.AddDays(3);

        // Fetch EventOccurrences for the next 3 days, including their associated Event to filter by FamilyId
        stats.UpcomingEventsCount = await _context.EventOccurrences
            .Where(eo => !eo.Event.IsDeleted && eo.Event.FamilyId == request.FamilyId) // Filter by accessible families and non-deleted events
            .Where(eo => eo.OccurrenceDate.Date >= today && eo.OccurrenceDate.Date <= threeDaysLater)
            .Select(eo => eo.EventId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Calculate Living/Deceased Members
        stats.LivingMembersCount = data.Members.Count(m => !m.IsDeceased);
        stats.DeceasedMembersCount = data.Members.Count(m => m.IsDeceased);

        // Calculate Gender Ratio
        var totalMembersForGender = data.Members.Count(m => !string.IsNullOrEmpty(m.Gender));
        var maleCount = data.Members.Count(m => m.Gender == Gender.Male.ToString());
        var femaleCount = data.Members.Count(m => m.Gender == Gender.Female.ToString());
        stats.MaleRatio = totalMembersForGender > 0 ? Math.Round((double)maleCount / totalMembersForGender, 2) : 0.0;
        stats.FemaleRatio = totalMembersForGender > 0 ? Math.Round((double)femaleCount / totalMembersForGender, 2) : 0.0;

        // Calculate Average Age
        var livingMembersWithBirthDate = data.Members
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

        foreach (var family in data.FamiliesInScope) // Sử dụng FamiliesInScope từ dữ liệu đã lọc
        {
            var filteredFamilyMembers = data.Members.Where(m => m.FamilyId == family.Id).ToList();
            var filteredFamilyRelationships = data.Relationships.Where(r => r.FamilyId == family.Id).ToList();

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

        // Calculate Storage Usage and Limit
        double totalUsedStorageMb = 0;
        double totalMaxStorageMb = 0;

        // Calculate Family Limits
        int totalMaxMembers = 0;
        int totalAiChatMonthlyLimit = 0;
        int totalAiChatMonthlyUsage = 0;

        foreach (var family in data.FamiliesInScope)
        {
            var familyLimitConfigResult = await _mediator.Send(new Families.Queries.GetFamilyLimitConfigurationQuery { FamilyId = family.Id }, cancellationToken);

            if (familyLimitConfigResult.IsSuccess && familyLimitConfigResult.Value != null)
            {
                totalMaxStorageMb += familyLimitConfigResult.Value.MaxStorageMb;

                // Calculate current storage for this family
                var currentFamilyStorageBytes = await _context.FamilyMedia
                    .Where(fm => fm.FamilyId == family.Id)
                    .SumAsync(fm => fm.FileSize, cancellationToken);

                totalUsedStorageMb += (double)currentFamilyStorageBytes / (1024 * 1024); // Convert bytes to MB

                totalMaxMembers += familyLimitConfigResult.Value.MaxMembers;
                totalAiChatMonthlyLimit += familyLimitConfigResult.Value.AiChatMonthlyLimit;
                totalAiChatMonthlyUsage += familyLimitConfigResult.Value.AiChatMonthlyUsage;
            }
        }

        stats.UsedStorageMb = totalUsedStorageMb;
        stats.MaxStorageMb = totalMaxStorageMb;

        // Calculate Family Limits
        stats.MaxMembers = totalMaxMembers;
        stats.AiChatMonthlyLimit = totalAiChatMonthlyLimit;
        stats.AiChatMonthlyUsage = totalAiChatMonthlyUsage;

        return Result<DashboardStatsDto>.Success(stats);
    }

    // Phương thức riêng tư để lọc và lấy dữ liệu
    private async Task<FilteredDashboardData> _GetFilteredDashboardData(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
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

        var familiesInScope = await filteredFamiliesQuery.ToListAsync(cancellationToken);

        return new FilteredDashboardData(filteredFamiliesQuery, members, relationships, events, familiesInScope);
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
