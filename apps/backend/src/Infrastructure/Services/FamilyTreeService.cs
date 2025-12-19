using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Domain.ValueObjects; // Add this
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace backend.Infrastructure.Services;

public class FamilyTreeService : IFamilyTreeService
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<FamilyTreeService> _localizer;

    public FamilyTreeService(IApplicationDbContext context, IStringLocalizer<FamilyTreeService> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task UpdateFamilyStats(Guid familyId, CancellationToken cancellationToken = default)
    {
        var family = await _context.Families.FindAsync([familyId], cancellationToken);
        if (family == null) return;

        var memberCount = await _context.Members.CountAsync(m => m.FamilyId == familyId, cancellationToken);
        var eventCount = await _context.Events.CountAsync(e => e.FamilyId == familyId, cancellationToken);

        family.UpdateStats(memberCount, eventCount);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncMemberLifeEvents(Guid memberId, Guid familyId, DateOnly? dateOfBirth, DateOnly? dateOfDeath, string memberFullName, CancellationToken cancellationToken)
    {
        // Find existing birth and death events for the member
        var birthEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Birth && e.EventMembers.Any(em => em.MemberId == memberId), cancellationToken);

        var deathEvent = await _context.Events
            .Include(e => e.EventMembers)
            .FirstOrDefaultAsync(e => e.Type == EventType.Death && e.EventMembers.Any(em => em.MemberId == memberId), cancellationToken);

        // Handle Birth Event
        if (dateOfBirth.HasValue)
        {
            var birthDateTime = dateOfBirth.Value.ToDateTime(TimeOnly.MinValue);
            if (birthEvent != null)
            {
                // Update existing birth event
                // Use UpdateSolarEvent
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
            else
            {
                // Create new birth event
                // Use CreateSolarEvent
                var newBirthEvent = Event.CreateSolarEvent(
                    _localizer["Birth of {0}", memberFullName],
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

        // Handle Death Event
        if (dateOfDeath.HasValue)
        {
            var deathDateTime = dateOfDeath.Value.ToDateTime(TimeOnly.MinValue);
            if (deathEvent != null)
            {
                // Update existing death event
                // Use UpdateSolarEvent
                deathEvent.UpdateSolarEvent(
                    deathEvent.Name,
                    deathEvent.Code,
                    deathEvent.Description,
                    deathDateTime, // New SolarDate
                    RepeatRule.Yearly, // Death anniversaries are yearly
                    deathEvent.Type,
                    deathEvent.Color
                );
            }
            else
            {
                // Create new death event
                // Use CreateSolarEvent
                var newDeathEvent = Event.CreateSolarEvent(
                    _localizer["Death of {0}", memberFullName],
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
        else if (deathEvent != null)
        {
            // Remove existing death event if date is cleared
            _context.Events.Remove(deathEvent);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
