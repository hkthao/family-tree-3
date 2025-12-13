using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Identity.Specifications;

public class UsersByIdsSpec : Specification<User>
{
    public UsersByIdsSpec(List<Guid> userIds)
    {
        Query.Where(u => userIds.Contains(u.Id));
    }
}
