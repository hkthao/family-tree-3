using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyOrderingSpecification : Specification<Family>
{
    public FamilyOrderingSpecification(string? sortBy, string? sortOrder)
    {
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
