using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Specifications;

public class UserPushTokenByIdSpecification : Specification<UserPushToken>, ISingleResultSpecification<UserPushToken>
{
    public UserPushTokenByIdSpecification(Guid id)
    {
        Query.Where(t => t.Id == id);
    }
}
