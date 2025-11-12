using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Events.Specifications;

public class EventsByFamilyIdsSpec : Specification<Event>
{
    public EventsByFamilyIdsSpec(List<Guid> familyIds)
    {
        Query.Where(e => e.FamilyId.HasValue && familyIds.Contains(e.FamilyId.Value));
    }
}
