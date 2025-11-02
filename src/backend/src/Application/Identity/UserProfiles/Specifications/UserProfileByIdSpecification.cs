using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Identity.UserProfiles.Specifications;

public class UserProfileByIdSpecification : SingleResultSpecification<UserProfile>
{
    public UserProfileByIdSpecification(Guid userProfileId)
    {
        Query.Where(up => up.Id == userProfileId)
             .Include(up => up.UserPreference);
    }
}
