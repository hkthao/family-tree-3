using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByLocationSpecification : Specification<Event>
{
    public EventByLocationSpecification(string? location)
    {
        if (!string.IsNullOrEmpty(location))
        {
            Query.Where(e => e.Location != null && e.Location.Contains(location));
        }
    }
}
