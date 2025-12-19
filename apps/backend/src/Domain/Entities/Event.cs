using backend.Domain.Enums;
using backend.Domain.Events.Events;
using backend.Domain.ValueObjects; // Add this using statement

namespace backend.Domain.Entities;

/// <summary>
/// Represents an Event Aggregate Root.
/// Handles event details and associated members to ensure consistency.
/// Nó encapsulates các thực thể con như EventMember.
/// Những thay đổi đối với các thực thể con này chỉ nên được thực hiện thông qua Aggregate Root Event.
/// </summary>
public class Event : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }

    public CalendarType CalendarType { get; private set; }

    // Chỉ dùng cho Solar event
    public DateTime? SolarDate { get; private set; }

    // Chỉ dùng cho Lunar event
    public LunarDate? LunarDate { get; private set; }

    public RepeatRule RepeatRule { get; private set; }

    public EventType Type { get; private set; }
    public string? Color { get; private set; }

    public Guid? FamilyId { get; private set; }
    public Family? Family { get; private set; }

    private readonly HashSet<EventMember> _eventMembers = new();
    public IReadOnlyCollection<EventMember> EventMembers => _eventMembers;

    // Private constructor for EF Core and factory methods
    private Event() { }

    // Factory method for creating Solar events
    public static Event CreateSolarEvent(
        string name,
        string code,
        EventType type,
        DateTime solarDate,
        RepeatRule repeatRule,
        Guid? familyId,
        string? description = null,
        string? color = null
    )
    {
        // Validation: Solar event must have a SolarDate
        if (solarDate == default)
        {
            throw new ArgumentException("Solar event must have a valid SolarDate.", nameof(solarDate));
        }

        var eventEntity = new Event
        {
            Name = name,
            Code = code,
            Description = description,
            CalendarType = CalendarType.Solar,
            SolarDate = solarDate,
            LunarDate = null, // Ensure LunarDate is null for Solar events
            RepeatRule = repeatRule,
            Type = type,
            Color = color,
            FamilyId = familyId
        };
        eventEntity.AddDomainEvent(new EventCreatedEvent(eventEntity));
        return eventEntity;
    }

    // Factory method for creating Lunar events
    public static Event CreateLunarEvent(
        string name,
        string code,
        EventType type,
        LunarDate lunarDate,
        RepeatRule repeatRule,
        Guid? familyId,
        string? description = null,
        string? color = null
    )
    {
        // Validation: Lunar event must have a LunarDate
        if (lunarDate == null)
        {
            throw new ArgumentException("Lunar event must have a valid LunarDate.", nameof(lunarDate));
        }

        var eventEntity = new Event
        {
            Name = name,
            Code = code,
            Description = description,
            CalendarType = CalendarType.Lunar,
            SolarDate = null, // Ensure SolarDate is null for Lunar events
            LunarDate = lunarDate,
            RepeatRule = repeatRule,
            Type = type,
            Color = color,
            FamilyId = familyId
        };
        eventEntity.AddDomainEvent(new EventCreatedEvent(eventEntity));
        return eventEntity;
    }

    // Update method for Solar events
    public void UpdateSolarEvent(
        string name,
        string code,
        string? description,
        DateTime solarDate,
        RepeatRule repeatRule,
        EventType type,
        string? color
    )
    {
        // Validation: Must be a Solar event
        if (CalendarType != CalendarType.Solar)
        {
            throw new InvalidOperationException("Cannot update Solar properties for a Lunar event.");
        }
        if (solarDate == default)
        {
            throw new ArgumentException("Solar event must have a valid SolarDate.", nameof(solarDate));
        }

        Name = name;
        Code = code;
        Description = description;
        SolarDate = solarDate;
        RepeatRule = repeatRule;
        Type = type;
        Color = color;
        AddDomainEvent(new EventUpdatedEvent(this));
    }

    // Update method for Lunar events
    public void UpdateLunarEvent(
        string name,
        string code,
        string? description,
        LunarDate lunarDate,
        RepeatRule repeatRule,
        EventType type,
        string? color
    )
    {
        // Validation: Must be a Lunar event
        if (CalendarType != CalendarType.Lunar)
        {
            throw new InvalidOperationException("Cannot update Lunar properties for a Solar event.");
        }
        if (lunarDate == null)
        {
            throw new ArgumentException("Lunar event must have a valid LunarDate.", nameof(lunarDate));
        }

        Name = name;
        Code = code;
        Description = description;
        LunarDate = lunarDate;
        RepeatRule = repeatRule;
        Type = type;
        Color = color;
        AddDomainEvent(new EventUpdatedEvent(this));
    }

    public void AddEventMember(Guid memberId)
    {
        if (_eventMembers.Any(em => em.MemberId == memberId))
        {
            throw new InvalidOperationException($"Member with ID {memberId} is already part of this event.");
        }
        _eventMembers.Add(new EventMember(Id, memberId));
    }

    public void RemoveEventMember(Guid memberId)
    {
        var eventMember = _eventMembers.FirstOrDefault(em => em.MemberId == memberId);
        if (eventMember != null)
        {
            _eventMembers.Remove(eventMember);
        }
    }

    public void ClearEventMembers()
    {
        _eventMembers.Clear();
    }
}
