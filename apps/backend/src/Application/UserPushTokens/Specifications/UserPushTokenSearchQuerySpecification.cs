using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.UserPushTokens.Specifications;

public class UserPushTokenSearchQuerySpecification : Specification<UserPushToken>
{
    public UserPushTokenSearchQuerySpecification(string searchQuery)
    {
        Query.Where(upt => upt.ExpoPushToken.Contains(searchQuery) ||
                           upt.Platform.Contains(searchQuery) ||
                           upt.DeviceId.Contains(searchQuery));
    }
}
