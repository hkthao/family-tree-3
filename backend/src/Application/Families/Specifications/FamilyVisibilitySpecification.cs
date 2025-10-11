using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications
{
    public class FamilyVisibilitySpecification : Specification<Family>
    {
        public FamilyVisibilitySpecification(string? visibility)
        {
            if (!string.IsNullOrEmpty(visibility))
            {
                Query.Where(f => f.Visibility == visibility);
            }
        }
    }
}
