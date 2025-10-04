using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyFilterSpecification : BaseSpecification<Family>
{
    public FamilyFilterSpecification(string? searchTerm, int skip, int take, string? sortBy, string? sortOrder)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            AddCriteria(family => family.Name.Contains(searchTerm) || (family.Description != null && family.Description.Contains(searchTerm)));
        }

        ApplyPaging(skip, take);

        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    if (sortOrder == "desc")
                        AddOrderByDescending(family => family.Name);
                    else
                        AddOrderBy(family => family.Name);
                    break;
                case "totalmembers":
                    if (sortOrder == "desc")
                        AddOrderByDescending(family => family.TotalMembers);
                    else
                        AddOrderBy(family => family.TotalMembers);
                    break;
                case "created":
                    if (sortOrder == "desc")
                        AddOrderByDescending(family => family.Created);
                    else
                        AddOrderBy(family => family.Created);
                    break;
                default:
                    AddOrderBy(family => family.Name); // Default sort by name
                    break;
            }
        }
        else
        {
            AddOrderBy(family => family.Name); // Default order by
        }
    }
}