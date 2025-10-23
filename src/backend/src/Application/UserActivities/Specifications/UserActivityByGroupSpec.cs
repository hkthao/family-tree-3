using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserActivities.Specifications;

/// <summary>
/// Specification to filter user activities by GroupId (e.g., FamilyId).
/// </summary>
public class UserActivityByGroupSpec : Specification<UserActivity>
{
    public UserActivityByGroupSpec(Guid? groupId)
    {
        if (groupId.HasValue)
        {
            Query.Where(ua => ua.GroupId == groupId.Value);
        }
    }
}
