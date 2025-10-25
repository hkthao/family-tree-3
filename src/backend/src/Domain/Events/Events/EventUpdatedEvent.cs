using backend.Domain.Entities;

namespace backend.Domain.Events.Events;

public class EventUpdatedEvent(Event @event) : BaseEvent
{
    public Event Event { get; } = @event;
}
