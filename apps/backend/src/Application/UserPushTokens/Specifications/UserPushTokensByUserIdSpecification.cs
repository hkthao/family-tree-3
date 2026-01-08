using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Specifications;

public class UserPushTokensByUserIdSpecification : Specification<UserPushToken>
{
    public UserPushTokensByUserIdSpecification(Guid userId)
    {
        Query.Where(t => t.UserId == userId);
    }
}
