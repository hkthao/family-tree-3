using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Specifications
{
    public class EventTypeSpecification : Specification<Event>
    {
        public EventTypeSpecification(string? type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                if (Enum.TryParse<EventType>(type, true, out var eventType))
                {
                    Query.Where(e => e.Type == eventType);
                }
            }
        }
    }
}
