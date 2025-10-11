using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserActivities.Specifications
{
    /// <summary>
    /// Specification to filter user activities by UserProfileId.
    /// </summary>
    public class UserActivityByProfileIdSpec : Specification<UserActivity>
    {
        public UserActivityByProfileIdSpec(Guid userProfileId)
        {
            Query.Where(ua => ua.UserProfileId == userProfileId);
        }
    }
}
