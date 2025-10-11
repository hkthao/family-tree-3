using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications
{
    public class MemberSearchTermSpecification : Specification<Member>
    {
        public MemberSearchTermSpecification(string? searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {
                Query.Where(m => m.FirstName.Contains(searchTerm) || m.LastName.Contains(searchTerm) || (m.Nickname != null && m.Nickname.Contains(searchTerm)));
            }
        }
    }
}
