using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByIdSpecification : Specification<Event>, ISingleResultSpecification<Event>
{
    public EventByIdSpecification(Guid id)
    {
        Query.Where(e => e.Id == id);
    }
}
