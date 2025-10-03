using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyFilterSpecification : BaseSpecification<Family>
{
    public FamilyFilterSpecification(string? searchTerm, DateTime? createdAfter, int skip, int take)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            AddCriteria(family => family.Name.Contains(searchTerm) || (family.Description != null && family.Description.Contains(searchTerm)));
        }

        if (createdAfter.HasValue)
        {
            AddCriteria(family => family.Created > createdAfter.Value);
        }

        ApplyPaging(skip, take);
        AddOrderBy(family => family.Name); // Default order by
    }
}