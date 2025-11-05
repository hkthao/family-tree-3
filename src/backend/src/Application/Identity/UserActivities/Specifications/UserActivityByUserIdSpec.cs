using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserActivities.Specifications;

/// <summary>
/// Specification to filter user activities by UserId.
/// </summary>
public class UserActivityByUserIdSpec : Specification<UserActivity>
{
    public UserActivityByUserIdSpec(Guid userId)
    {
        Query.Where(ua => ua.UserId == userId);
    }
}
