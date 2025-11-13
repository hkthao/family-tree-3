using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Users.Specifications;

public class UserSearchTermSpecification : Specification<User>
{
    public UserSearchTermSpecification(string? searchTerm)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Query.Where(u => u.Profile!.Name.Contains(searchTerm) || u.Email.Contains(searchTerm));
        }
    }
}
