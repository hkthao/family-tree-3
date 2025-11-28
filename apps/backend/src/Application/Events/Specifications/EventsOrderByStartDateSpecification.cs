using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsOrderByStartDateSpecification : Specification<Event>
{
    public EventsOrderByStartDateSpecification(string sortOrder)
    {
        if (sortOrder == "desc")
        {
            Query.OrderByDescending(e => e.StartDate);
        }
        else
        {
            Query.OrderBy(e => e.StartDate);
        }
    }
}
