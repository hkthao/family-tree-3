using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Application.Members.Queries.SearchMembers;

namespace backend.Application.Members.Specifications;

public class MemberFilterSpecification : Specification<Member>
{
    public MemberFilterSpecification(SearchMembersQuery query, bool includeRelationships = false)
    {
        if (includeRelationships)
        {
            Query.Include(m => m.Relationships);
        }

        if (!string.IsNullOrEmpty(query.SearchQuery))
        {
            Query.Where(m => m.FirstName.Contains(query.SearchQuery) || m.LastName.Contains(query.SearchQuery) || (m.Nickname != null && m.Nickname.Contains(query.SearchQuery)));
        }

        if (!string.IsNullOrEmpty(query.Gender))
        {
            Query.Where(m => m.Gender == query.Gender);
        }

        if (query.FamilyId.HasValue)
        {
            Query.Where(m => m.FamilyId == query.FamilyId.Value);
        }

        Query.Skip((query.Page - 1) * query.ItemsPerPage).Take(query.ItemsPerPage);

        if (!string.IsNullOrEmpty(query.SortBy))
        {
            switch (query.SortBy.ToLower())
            {
                case "firstname":
                    if (query.SortOrder == "desc")
                        Query.OrderByDescending(member => member.FirstName);
                    else
                        Query.OrderBy(member => member.FirstName);
                    break;
                case "lastname":
                    if (query.SortOrder == "desc")
                        Query.OrderByDescending(member => member.LastName);
                    else
                        Query.OrderBy(member => member.LastName);
                    break;
                case "fullname":
                    if (query.SortOrder == "desc")
                    {
                        Query.OrderByDescending(member => member.FirstName);
                        Query.OrderByDescending(member => member.LastName);
                    }
                    else
                    {
                        Query.OrderBy(member => member.FirstName);
                        Query.OrderBy(member => member.LastName);
                    }
                    break;
                case "dateofbirth":
                    if (query.SortOrder == "desc")
                        Query.OrderByDescending(member => member.DateOfBirth!);
                    else
                        Query.OrderBy(member => member.DateOfBirth!);
                    break;
                case "gender":
                    if (query.SortOrder == "desc")
                        Query.OrderByDescending(member => member.Gender!);
                    else
                        Query.OrderBy(member => member.Gender!);
                    break;
                case "created":
                    if (query.SortOrder == "desc")
                        Query.OrderByDescending(member => member.Created);
                    else
                        Query.OrderBy(member => member.Created);
                    break;
                default:
                    Query.OrderBy(member => member.FirstName);
                    Query.OrderBy(member => member.LastName);
                    break;
            }
        }
        else
        {
            Query.OrderBy(member => member.FirstName);
            Query.OrderBy(member => member.LastName);
        }
    }
}
