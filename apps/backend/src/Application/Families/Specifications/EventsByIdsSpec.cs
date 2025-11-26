using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsByIdsSpec : Specification<Event>
{
    public EventsByIdsSpec(List<Guid> eventIds)
    {
        Query.Where(e => eventIds.Contains(e.Id));
    }
}
