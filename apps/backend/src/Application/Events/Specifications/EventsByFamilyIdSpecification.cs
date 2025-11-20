using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsByFamilyIdSpecification : Specification<Event>
{
    public EventsByFamilyIdSpecification(Guid familyId)
    {
        Query.Where(e => e.FamilyId == familyId);
    }
}
