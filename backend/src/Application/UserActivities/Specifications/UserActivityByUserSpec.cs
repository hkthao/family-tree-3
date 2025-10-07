using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Specifications;

/// <summary>
/// Specification to filter user activities by UserProfileId, and optionally by TargetType, TargetId, and FamilyId.
/// Includes ordering by Timestamp descending and limits the number of results.
/// </summary>
public class UserActivityByUserSpec : Specification<UserActivity>
{
    public UserActivityByUserSpec(Guid userProfileId, int limit, TargetType? targetType = null, string? targetId = null, Guid? groupId = null)
    {
        Query
            .Where(ua => ua.UserProfileId == userProfileId)
            .OrderByDescending(ua => ua.Created)
            .Take(limit);

        if (targetType.HasValue)
        {
            Query.Where(ua => ua.TargetType == targetType.Value);
        }

        if (!string.IsNullOrEmpty(targetId))
        {
            Query.Where(ua => ua.TargetId == targetId);
        }

        if (groupId.HasValue)
        {
            Query.Where(ua => ua.GroupId == groupId.Value);
        }
    }
}
