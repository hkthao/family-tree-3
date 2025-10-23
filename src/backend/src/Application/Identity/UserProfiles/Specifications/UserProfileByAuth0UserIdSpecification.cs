using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserProfiles.Specifications;

public class UserProfileByExternalIdSpecification : SingleResultSpecification<UserProfile>
{
    public UserProfileByExternalIdSpecification(string externalId)
    {
        Query.Where(up => up.ExternalId == externalId);
    }
}
