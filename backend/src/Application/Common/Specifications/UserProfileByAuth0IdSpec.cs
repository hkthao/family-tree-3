using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Common.Specifications;

/// <summary>
/// Specification to retrieve a UserProfile by Auth0UserId, including associated FamilyUsers.
/// </summary>
public class UserProfileByAuth0IdSpec : Specification<UserProfile>, ISingleResultSpecification<UserProfile>
{
    public UserProfileByAuth0IdSpec(string auth0UserId)
    {
        Query
            .Where(up => up.Auth0UserId == auth0UserId)
            .Include(up => up.FamilyUsers);
    }
}
