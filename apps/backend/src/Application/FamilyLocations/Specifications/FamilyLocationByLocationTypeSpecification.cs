using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationByLocationTypeSpecification : Specification<FamilyLocation>
{
    public FamilyLocationByLocationTypeSpecification(LocationType? locationType)
    {
        if (locationType.HasValue)
        {
            Query.Where(l => l.LocationType == locationType.Value);
        }
    }
}
