using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Events.Specifications;

public class PublicEventsSpecification : Specification<Event>
{
    public PublicEventsSpecification()
    {
        Query.Where(e => e.Family != null && e.Family.Visibility == FamilyVisibility.Public.ToString());
    }
}
