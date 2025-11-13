using backend.Domain.Enums;
using backend.Domain.Events.Events;

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
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Location { get; private set; }
    public Guid? FamilyId { get; private set; }
    public EventType Type { get; private set; }
    public string? Color { get; private set; }

    private readonly HashSet<EventMember> _eventMembers = new();
    public IReadOnlyCollection<EventMember> EventMembers => _eventMembers;

    // Private constructor for EF Core
    private Event() { }

    public Event(string name, string code, EventType type, Guid? familyId)
    {
        Name = name;
        Code = code;
        Type = type;
        FamilyId = familyId;
    }

    public Event(string name, string code, EventType type, Guid? familyId, DateTime? startDate) : this(name, code, type, familyId)
    {
        StartDate = startDate;
    }

    public void UpdateEvent(string name, string code, string? description, DateTime? startDate, DateTime? endDate, string? location, EventType type, string? color)
    {
        Name = name;
        Code = code;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
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
