using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLocations.Specifications;

public class FamilyLocationSearchTermSpecification : Specification<FamilyLocation>
{
    public FamilyLocationSearchTermSpecification(string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            Query.Where(l => l.Name.Contains(searchTerm) ||
                             (l.Description != null && l.Description.Contains(searchTerm)) ||
                             (l.Address != null && l.Address.Contains(searchTerm)));
        }
    }
}
