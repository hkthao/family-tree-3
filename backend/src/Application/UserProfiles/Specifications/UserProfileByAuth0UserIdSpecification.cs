using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserProfiles.Specifications;

public class UserProfileByAuth0UserIdSpecification : SingleResultSpecification<UserProfile>
{
    public UserProfileByAuth0UserIdSpecification(string auth0UserId)
    {
        Query.Where(up => up.ExternalId == auth0UserId);
    }
}
