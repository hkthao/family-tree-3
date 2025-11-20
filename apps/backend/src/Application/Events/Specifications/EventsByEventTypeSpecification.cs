using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Specifications;

public class EventsByEventTypeSpecification : Specification<Event>
{
    public EventsByEventTypeSpecification(EventType eventType)
    {
        Query.Where(e => e.Type == eventType);
    }
}
