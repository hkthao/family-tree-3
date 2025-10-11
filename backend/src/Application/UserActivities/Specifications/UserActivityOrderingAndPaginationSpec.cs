using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserActivities.Specifications
{
    /// <summary>
    /// Specification to handle ordering by Timestamp descending and limiting the number of results.
    /// </summary>
    public class UserActivityOrderingAndPaginationSpec : Specification<UserActivity>
    {
        public UserActivityOrderingAndPaginationSpec(int limit)
        {
            Query
                .OrderByDescending(ua => ua.Created)
                .Take(limit);
        }
    }
}
