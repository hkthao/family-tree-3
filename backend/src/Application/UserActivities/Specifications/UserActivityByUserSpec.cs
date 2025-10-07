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
    public UserActivityByUserSpec(Guid userProfileId, int limit, TargetType? targetType = null, Guid? targetId = null, Guid? familyId = null)
    {
        Query
            .Where(ua => ua.UserProfileId == userProfileId)
            .OrderByDescending(ua => ua.Created)
            .Take(limit);

        if (targetType.HasValue)
        {
            Query.Where(ua => ua.TargetType == targetType.Value);
        }

        if (targetId.HasValue)
        {
            Query.Where(ua => ua.TargetId == targetId.Value);
        }

        // Note: Filtering by FamilyId would require joining with FamilyUser or having FamilyId directly on UserActivity.
        // For now, assuming TargetId might be a FamilyId if TargetType is Family.
        // If more complex filtering by family is needed, UserActivity entity might need to be extended.
    }
}
