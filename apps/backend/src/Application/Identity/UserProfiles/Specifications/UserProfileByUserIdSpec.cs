using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Identity.UserProfiles.Specifications;

public class UserProfileByUserIdSpec : SingleResultSpecification<UserProfile>
{
    public UserProfileByUserIdSpec(Guid userId)
    {
        Query.Where(up => up.UserId == userId);
    }
}
