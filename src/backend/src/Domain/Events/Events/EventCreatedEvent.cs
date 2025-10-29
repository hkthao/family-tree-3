using backend.Domain.Entities;

namespace backend.Domain.Events.Events;

public class EventCreatedEvent(Event @event) : BaseEvent
{
    public Event Event { get; } = @event;
}
