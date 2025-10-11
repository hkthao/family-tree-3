using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Common.Specifications
{
    /// <summary>
    /// Specification to retrieve a UserProfile by ExternalId, including associated FamilyUsers.
    /// </summary>
    public class UserProfileByAuth0IdSpec : Specification<UserProfile>, ISingleResultSpecification<UserProfile>
    {
        public UserProfileByAuth0IdSpec(string externalId)
        {
            Query
                .Where(up => up.ExternalId == externalId)
                .Include(up => up.FamilyUsers);
        }
    }
}
