using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventByFamilyIdSpecification : Specification<Event>
{
    public EventByFamilyIdSpecification(Guid? familyId)
    {
        if (familyId.HasValue)
        {
            Query.Where(e => e.FamilyId == familyId.Value);
        }
    }
}
