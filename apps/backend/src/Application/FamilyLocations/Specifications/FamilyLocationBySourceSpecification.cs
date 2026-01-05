using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationBySourceSpecification : Specification<FamilyLocation>
{
    public FamilyLocationBySourceSpecification(LocationSource? source)
    {
        if (source.HasValue)
        {
            Query.Where(l => l.Location.Source == source.Value);
        }
    }
}
