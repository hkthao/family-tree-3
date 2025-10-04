using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyFilterSpecification : Specification<Family>
{
    public FamilyFilterSpecification(string? searchTerm, int skip, int take, string? sortBy, string? sortOrder)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Query.Where(f => f.Name.Contains(searchTerm) || (f.Description != null && f.Description.Contains(searchTerm)));
        }

        Query.Skip(skip).Take(take);

        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "name":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(family => family.Name);
                    else
                        Query.OrderBy(family => family.Name);
                    break;
                case "totalmembers":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(family => family.TotalMembers);
                    else
                        Query.OrderBy(family => family.TotalMembers);
                    break;
                case "created":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(family => family.Created);
                    else
                        Query.OrderBy(family => family.Created);
                    break;
                default:
                    Query.OrderBy(family => family.Name); // Default sort by name
                    break;
            }
        }
        else
        {
            Query.OrderBy(family => family.Name); // Default order by
        }
    }
}