using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventCreatedAfterSpecification : BaseSpecification<Event>
{
    public EventCreatedAfterSpecification(DateTime date)
    {
        AddCriteria(e => e.Created > date);
    }
}