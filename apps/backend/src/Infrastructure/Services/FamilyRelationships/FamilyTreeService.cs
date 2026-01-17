using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
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
        var family = await _context.Families
            .Include(f => f.Members.Where(m => !m.IsDeleted)) // Load only non-deleted members
            .Include(f => f.Relationships.Where(r => !r.IsDeleted)) // Load only non-deleted relationships
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family == null) return;

        family.RecalculateStats(); // This will update TotalMembers and TotalGenerations
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
                // If it's a Lunar event, we don't update it with a Solar date here.
                // Further logic would be needed if Lunar dates for members were directly supported and needed syncing.
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
                if (deathEvent.CalendarType == CalendarType.Solar)
                {
                    // Update existing death event (Solar)
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
                // If it's a Lunar event, we don't update it with a Solar date here.
                // Further logic would be needed if Lunar dates for members were directly supported and needed syncing.
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
