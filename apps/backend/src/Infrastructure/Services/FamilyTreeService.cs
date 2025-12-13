using backend.Application.Common.Constants;
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
                birthEvent.UpdateEvent(birthEvent.Name, birthEvent.Code, birthEvent.Description, birthDateTime, birthEvent.EndDate, birthEvent.Location, birthEvent.Type, birthEvent.Color);
            }
            else
            {
                // Create new birth event
                var newBirthEvent = new Event(_localizer["Birth of {0}", memberFullName], $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}", EventType.Birth, familyId, birthDateTime);
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
                deathEvent.UpdateEvent(deathEvent.Name, deathEvent.Code, deathEvent.Description, deathDateTime, deathEvent.EndDate, deathEvent.Location, deathEvent.Type, deathEvent.Color);
            }
            else
            {
                // Create new death event
                var newDeathEvent = new Event(_localizer["Death of {0}", memberFullName], $"EVT-{Guid.NewGuid().ToString()[..5].ToUpper()}", EventType.Death, familyId, deathDateTime);
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
