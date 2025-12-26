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

            Query.Where(l => l.Name.ToLower().Contains(lowerSearchQuery) || // Convert Name to lowercase
                             (l.Description != null && l.Description.ToLower().Contains(lowerSearchQuery)) || // Convert Description to lowercase
                             (l.Address != null && l.Address.ToLower().Contains(lowerSearchQuery))); // Convert Address to lowercase
        }
    }
}
