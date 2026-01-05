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
                        Query.OrderByDescending(l => l.Location.Name);
                    else
                        Query.OrderBy(l => l.Location.Name);
                    break;
                case "locationtype":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.Location.LocationType);
                    else
                        Query.OrderBy(l => l.Location.LocationType);
                    break;
                case "accuracy":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.Location.Accuracy);
                    else
                        Query.OrderBy(l => l.Location.Accuracy);
                    break;

                case "source":
                    if (sortOrder?.ToLower() == "desc")
                        Query.OrderByDescending(l => l.Location.Source);
                    else
                        Query.OrderBy(l => l.Location.Source);
                    break;
                default:
                    Query.OrderBy(l => l.Location.Name); // Default sort
                    break;
            }
        }
        else
        {
            Query.OrderBy(l => l.Location.Name); // Default sort
        }
    }
}
