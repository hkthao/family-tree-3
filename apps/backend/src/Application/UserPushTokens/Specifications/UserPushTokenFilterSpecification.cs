using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Specifications;

public class UserPushTokenFilterSpecification : Specification<UserPushToken>
{
    public UserPushTokenFilterSpecification(Guid? userId, string? platform, bool? isActive)
    {
        if (userId.HasValue)
        {
            Query.Where(upt => upt.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(platform))
        {
            Query.Where(upt => upt.Platform == platform);
        }

        if (isActive.HasValue)
        {
            Query.Where(upt => upt.IsActive == isActive.Value);
        }
    }
}
