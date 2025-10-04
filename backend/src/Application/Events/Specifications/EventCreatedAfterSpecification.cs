using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventCreatedAfterSpecification : Specification<Event>
{
    public EventCreatedAfterSpecification(DateTime date)
    {
        Query.Where(e => e.Created > date);
    }
}