using backend.Application.Common.Specifications;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberFilterSpecification : BaseSpecification<Member>
{
    public MemberFilterSpecification(string? searchTerm, DateTime? createdAfter, int skip, int take)
    {
        if (!string.IsNullOrEmpty(searchTerm))
        {
            AddCriteria(m => m.FirstName.Contains(searchTerm) || m.LastName.Contains(searchTerm));
        }

        if (createdAfter.HasValue)
        {
            AddCriteria(m => m.Created > createdAfter.Value);
        }

        ApplyPaging(skip, take);
        AddOrderBy(m => m.LastName); // Default order by
    }
}