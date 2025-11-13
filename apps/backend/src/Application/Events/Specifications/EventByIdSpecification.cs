using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByIdSpecification : Specification<Event>, ISingleResultSpecification<Event>
{
    public EventByIdSpecification(Guid id, bool includeMembers = false)
    {
        Query.Where(e => e.Id == id);
        if (includeMembers)
        {
            Query.Include(e => e.EventMembers);
        }
    }
}
