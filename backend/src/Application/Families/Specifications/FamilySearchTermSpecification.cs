using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications
{
    public class FamilySearchTermSpecification : Specification<Family>
    {
        public FamilySearchTermSpecification(string? searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                Query.Where(f => f.Name.Contains(searchTerm) || (f.Description != null && f.Description.Contains(searchTerm)));
            }
        }
    }
}
