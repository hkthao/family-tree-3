using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberSearchTermSpecification : Specification<Member>
{
    public MemberSearchTermSpecification(string? searchTerm)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            Query.Where(m => m.FirstName.ToLower().Contains(searchTerm.ToLower()) || m.LastName.ToLower().Contains(searchTerm.ToLower()) || (m.Nickname != null && m.Nickname.ToLower().Contains(searchTerm.ToLower())));
        }
    }
}
