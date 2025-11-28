using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventOrderByStartDateSpec : Specification<Event>
{
    public EventOrderByStartDateSpec()
    {
        Query.OrderBy(e => e.StartDate);
    }
}
