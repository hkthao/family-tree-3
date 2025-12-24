using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Users.Specifications;

public class UserSearchQuerySpecification : Specification<User>
{
    public UserSearchQuerySpecification(string? searchTerm)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Query.Where(u => u.Profile!.Name.Contains(searchTerm) || u.Email.Contains(searchTerm));
        }
    }
}
