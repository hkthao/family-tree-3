using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Dashboard.Specifications;

public class FamilyUsersByUserIdSpec : Specification<FamilyUser>
{
    public FamilyUsersByUserIdSpec(Guid userId)
    {
        Query.Where(fu => fu.UserId == userId);
    }
}
