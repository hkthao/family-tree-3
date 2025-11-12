using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamiliesByIdsSpecification : Specification<Family>
{
    public FamiliesByIdsSpecification(List<Guid> ids)
    {
        Query.Where(f => ids.Contains(f.Id));
    }
}
