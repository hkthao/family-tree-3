using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;


namespace backend.Infrastructure.Services;

public class FamilyTreeService : IFamilyTreeService
{
    private readonly IApplicationDbContext _context;

    public FamilyTreeService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default)
    {
        var family = await _context.Families
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null) return;

        // Optimized: Count members directly from the database
        family.TotalMembers = await _context.Members
            .CountAsync(m => m.FamilyId == familyId && !m.IsDeleted, cancellationToken);

        // Optimized: Calculate TotalGenerations without loading all entities
        // 1. Fetch only necessary data (IDs and relationship types)
        var memberIds = await _context.Members
            .AsNoTracking() // Add AsNoTracking for read-only query
            .Where(m => m.FamilyId == familyId && !m.IsDeleted)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var relationships = await _context.Relationships
            .AsNoTracking() // Add AsNoTracking for read-only query
            .Where(r => r.FamilyId == familyId && !r.IsDeleted && (r.Type == RelationshipType.Father || r.Type == RelationshipType.Mother))
            .Select(r => new { r.SourceMemberId, r.TargetMemberId })
            .ToListAsync(cancellationToken);

        // 2. Build a simplified graph in memory
        var graph = new Dictionary<Guid, List<Guid>>();
        var parents = new Dictionary<Guid, List<Guid>>();

        foreach (var memberId in memberIds)
        {
            graph[memberId] = new List<Guid>();
            parents[memberId] = new List<Guid>();
        }

        foreach (var rel in relationships)
        {
            if (memberIds.Contains(rel.SourceMemberId) && memberIds.Contains(rel.TargetMemberId)) // Ensure both members exist and are not deleted
            {
                if (graph.ContainsKey(rel.SourceMemberId))
                {
                    graph[rel.SourceMemberId].Add(rel.TargetMemberId);
                }
                if (parents.ContainsKey(rel.TargetMemberId))
                {
                    parents[rel.TargetMemberId].Add(rel.SourceMemberId);
                }
            }
        }

        // 3. Find all root members (members with no parents in the filtered relationships)
        var rootMembers = memberIds.Where(mId => !parents.ContainsKey(mId) || parents[mId].Count == 0).ToList();

        // If no explicit roots found via relationships, consider members who are not target of any parent relationship as roots
        if (rootMembers.Count == 0 && memberIds.Any())
        {
            rootMembers = memberIds.Where(mId => !relationships.Any(r => r.TargetMemberId == mId)).ToList();
        }

        // Fallback: if still no roots, and there are members, pick the first member (arbitrary root for generation calculation)
        if (rootMembers.Count == 0 && memberIds.Any())
        {
            rootMembers.Add(memberIds.First());
        }

        int maxGenerations = 0;
        foreach (var root in rootMembers)
        {
            maxGenerations = Math.Max(maxGenerations, GetGenerations(root, graph, new HashSet<Guid>()));
        }

        family.TotalGenerations = maxGenerations;

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Helper method for calculating generations (moved from Family entity)
    private int GetGenerations(Guid memberId, Dictionary<Guid, List<Guid>> graph, HashSet<Guid> visited)
    {
        if (visited.Contains(memberId)) return 0; // Avoid infinite loops in case of circular relationships
        visited.Add(memberId);

        if (!graph.ContainsKey(memberId) || graph[memberId].Count == 0)
        {
            return 1; // Base case: leaf member is 1 generation
        }

        int maxChildGenerations = 0;
        foreach (var childId in graph[memberId])
        {
            maxChildGenerations = Math.Max(maxChildGenerations, GetGenerations(childId, graph, visited));
        }

        return 1 + maxChildGenerations;
    }

    public async Task SyncMemberLifeEvents(Guid memberId, Guid familyId, DateOnly? dateOfBirth, DateOnly? dateOfDeath, LunarDate? lunarDateOfDeath, string memberFullName, CancellationToken cancellationToken)
    {
        // Find existing birth and death events for the member
        var birthEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Birth && e.EventMembers.Any(em => em.MemberId == memberId), cancellationToken);

        var solarDeathEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Solar && e.EventMembers.Any(em => em.MemberId == memberId), cancellationToken);

        var lunarDeathEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Death && e.CalendarType == CalendarType.Lunar && e.EventMembers.Any(em => em.MemberId == memberId), cancellationToken);

        // Handle Birth Event
        if (dateOfBirth.HasValue)
        {
            var birthDateTime = dateOfBirth.Value.ToDateTime(TimeOnly.MinValue);
            if (birthEvent != null)
            {
                if (birthEvent.CalendarType == CalendarType.Solar)
                {
                    // Update existing birth event (Solar)
                    birthEvent.UpdateSolarEvent(
                        birthEvent.Name,
                        birthEvent.Code,
                        birthEvent.Description,
                        birthDateTime, // New SolarDate
                        RepeatRule.Yearly, // Birthdays are yearly
                        birthEvent.Type,
                        birthEvent.Color
                    );
                }
            }
            else
            {
                // Create new birth event
                // Use CreateSolarEvent
                var newBirthEvent = Event.CreateSolarEvent(
                    "Ngày sinh của " + memberFullName,
                    $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
                    EventType.Birth,
                    birthDateTime, // SolarDate
                    RepeatRule.Yearly, // Birthdays are yearly
                    familyId
                );
                newBirthEvent.AddEventMember(memberId);
                _context.Events.Add(newBirthEvent);
            }
        }
        else if (birthEvent != null)
        {
            // Remove existing birth event if date is cleared
            _context.Events.Remove(birthEvent);
        }

        // Handle Solar Death Event
        if (dateOfDeath.HasValue)
        {
            var deathDateTime = dateOfDeath.Value.ToDateTime(TimeOnly.MinValue);
            if (solarDeathEvent != null)
            {
                // Update existing death event (Solar)
                solarDeathEvent.UpdateSolarEvent(
                    solarDeathEvent.Name,
                    solarDeathEvent.Code,
                    solarDeathEvent.Description,
                    deathDateTime, // New SolarDate
                    RepeatRule.Yearly, // Death anniversaries are yearly
                    solarDeathEvent.Type,
                    solarDeathEvent.Color
                );
            }
            else
            {
                // Create new death event
                // Use CreateSolarEvent
                var newDeathEvent = Event.CreateSolarEvent(
                    "Ngày mất của " + memberFullName,
                    $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
                    EventType.Death,
                    deathDateTime, // SolarDate
                    RepeatRule.Yearly, // Death anniversaries are yearly
                    familyId
                );
                newDeathEvent.AddEventMember(memberId);
                _context.Events.Add(newDeathEvent);
            }
        }
        else if (solarDeathEvent != null)
        {
            // Remove existing death event if date is cleared
            _context.Events.Remove(solarDeathEvent);
        }

        // Handle Lunar Death Event
        if (lunarDateOfDeath != null)
        {
            if (lunarDeathEvent != null)
            {
                // Update existing lunar death event
                lunarDeathEvent.UpdateLunarEvent(
                    lunarDeathEvent.Name,
                    lunarDeathEvent.Code,
                    lunarDeathEvent.Description,
                    lunarDateOfDeath, // New LunarDate
                    RepeatRule.Yearly, // Death anniversaries are yearly
                    lunarDeathEvent.Type,
                    lunarDeathEvent.Color
                );
            }
            else
            {
                // Create new lunar death event
                var newLunarDeathEvent = Event.CreateLunarEvent(
                    "Ngày giỗ của " + memberFullName, // Distinct name for lunar event
                    $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}",
                    EventType.Death,
                    lunarDateOfDeath, // LunarDate
                    RepeatRule.Yearly, // Death anniversaries are yearly
                    familyId
                );
                newLunarDeathEvent.AddEventMember(memberId);
                _context.Events.Add(newLunarDeathEvent);
            }
        }
        else if (lunarDeathEvent != null)
        {
            // Remove existing lunar death event if date is cleared
            _context.Events.Remove(lunarDeathEvent);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
