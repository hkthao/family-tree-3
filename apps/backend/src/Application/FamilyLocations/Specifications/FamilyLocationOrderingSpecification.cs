using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationOrderingSpecification : Specification<FamilyLocation>
{
    public FamilyLocationOrderingSpecification(string? sortBy, string? sortOrder)
    {
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.Name);
                    else
                        Query.OrderBy(l => l.Name);
                    break;
                case "locationtype":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.LocationType);
                    else
                        Query.OrderBy(l => l.LocationType);
                    break;
                case "accuracy":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.Accuracy);
                    else
                        Query.OrderBy(l => l.Accuracy);
                    break;

                case "source":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.Source);
                    else
                        Query.OrderBy(l => l.Source);
                    break;
                default:
                    Query.OrderBy(l => l.Name); // Default sort
                    break;
            }
        }
        else
        {
            Query.OrderBy(l => l.Name); // Default sort
        }
    }
}
