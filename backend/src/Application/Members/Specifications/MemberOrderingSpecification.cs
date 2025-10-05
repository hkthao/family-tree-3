using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberOrderingSpecification : Specification<Member>
{
    public MemberOrderingSpecification(string? sortBy, string? sortOrder)
    {
        if (!string.IsNullOrEmpty(sortBy))
        {
            switch (sortBy.ToLower())
            {
                case "firstname":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(member => member.FirstName);
                    else
                        Query.OrderBy(member => member.FirstName);
                    break;
                case "lastname":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(member => member.LastName);
                    else
                        Query.OrderBy(member => member.LastName);
                    break;
                case "fullname":
                    if (sortOrder == "desc")
                    {
                        Query.OrderByDescending(member => member.FirstName).ThenByDescending(member => member.LastName);
                    }
                    else
                    {
                        Query.OrderBy(member => member.FirstName).ThenBy(member => member.LastName);
                    }
                    break;
                case "dateofbirth":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(member => member.DateOfBirth!);
                    else
                        Query.OrderBy(member => member.DateOfBirth!);
                    break;
                case "gender":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(member => member.Gender!);
                    else
                        Query.OrderBy(member => member.Gender!);
                    break;
                case "created":
                    if (sortOrder == "desc")
                        Query.OrderByDescending(member => member.Created);
                    else
                        Query.OrderBy(member => member.Created);
                    break;
                default:
                    Query.OrderBy(member => member.FirstName).ThenBy(member => member.LastName); // Default sort by first name, then last name
                    break;
            }
        }
        else
        {
            Query.OrderBy(member => member.FirstName).ThenBy(member => member.LastName); // Default order by first name, then last name
        }
    }
}
