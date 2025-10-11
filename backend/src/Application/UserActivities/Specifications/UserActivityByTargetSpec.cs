using Ardalis.Specification;
using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.UserActivities.Specifications
{
    /// <summary>
    /// Specification to filter user activities by TargetType and TargetId.
    /// </summary>
    public class UserActivityByTargetSpec : Specification<UserActivity>
    {
        public UserActivityByTargetSpec(TargetType? targetType, string? targetId)
        {
            if (targetType.HasValue)
            {
                Query.Where(ua => ua.TargetType == targetType.Value);
            }

            if (!string.IsNullOrEmpty(targetId))
            {
                Query.Where(ua => ua.TargetId == targetId);
            }
        }
    }
}
