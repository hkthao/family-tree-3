using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByNameSpecification : BaseSpecification<Event>
{
    public EventByNameSpecification(string name)
    {
        AddCriteria(e => e.Name.Contains(name));
    }
}