using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserActivities.Specifications;

/// <summary>
/// Specification to filter user activities by GroupId.
/// </summary>
public class UserActivityByGroupIdSpec : Specification<UserActivity>
{
    public UserActivityByGroupIdSpec(Guid groupId)
    {
        Query.Where(ua => ua.GroupId == groupId);
    }
}
