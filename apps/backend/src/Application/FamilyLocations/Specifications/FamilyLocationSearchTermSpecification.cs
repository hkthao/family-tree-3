using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationSearchTermSpecification : Specification<FamilyLocation>
{
    public FamilyLocationSearchTermSpecification(string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower(); // Convert search term to lowercase

            Query.Where(l => l.Name.ToLower().Contains(lowerSearchTerm) || // Convert Name to lowercase
                             (l.Description != null && l.Description.ToLower().Contains(lowerSearchTerm)) || // Convert Description to lowercase
                             (l.Address != null && l.Address.ToLower().Contains(lowerSearchTerm))); // Convert Address to lowercase
        }
    }
}
