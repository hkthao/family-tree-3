using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsOrderByNameSpecification : Specification<Event>
{
    public EventsOrderByNameSpecification(string sortOrder)
    {
        if (sortOrder == "desc")
        {
            Query.OrderByDescending(e => e.Name);
        }
        else
        {
            Query.OrderBy(e => e.Name);
        }
    }
}
