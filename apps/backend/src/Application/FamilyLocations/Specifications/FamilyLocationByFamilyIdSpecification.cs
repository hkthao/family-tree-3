using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationByFamilyIdSpecification : Specification<FamilyLocation>
{
    public FamilyLocationByFamilyIdSpecification(Guid? familyId)
    {
        if (familyId.HasValue)
        {
            Query.Where(l => l.FamilyId == familyId.Value);
        }
    }
}
