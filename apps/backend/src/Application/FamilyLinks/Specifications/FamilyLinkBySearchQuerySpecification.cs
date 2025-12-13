using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.FamilyLinks.Specifications;

public class FamilyLinkBySearchQuerySpecification : Specification<FamilyLink>
{
    public FamilyLinkBySearchQuerySpecification(string searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            Query.Where(fl =>
                (fl.Family1.Name != null && fl.Family1.Name.Contains(searchQuery)) ||
                (fl.Family2.Name != null && fl.Family2.Name.Contains(searchQuery))
            );
        }
    }
}
