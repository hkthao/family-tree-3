using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationSearchQuerySpecification : Specification<FamilyLocation>
{
    public FamilyLocationSearchQuerySpecification(string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchQuery = searchTerm.ToLower(); // Convert search term to lowercase

            Query.Where(l => l.Location.Name.ToLower().Contains(lowerSearchQuery) || // Convert Name to lowercase
                             (l.Location.Description != null && l.Location.Description.ToLower().Contains(lowerSearchQuery)) || // Convert Description to lowercase
                             (l.Location.Address != null && l.Location.Address.ToLower().Contains(lowerSearchQuery))); // Convert Address to lowercase
        }
    }
}
