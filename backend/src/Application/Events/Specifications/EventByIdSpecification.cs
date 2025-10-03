using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByIdSpecification : BaseSpecification<Event>
{
    public EventByIdSpecification(Guid id)
    {
        AddCriteria(e => e.Id == id);
    }
}