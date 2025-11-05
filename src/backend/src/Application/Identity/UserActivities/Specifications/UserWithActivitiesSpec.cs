using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Common.Specifications;

public class UserWithActivitiesSpec : SingleResultSpecification<User>
{
    public UserWithActivitiesSpec(Guid userId)
    {
        Query.Where(u => u.Id == userId)
             .Include(u => u.UserActivities);
    }
}
