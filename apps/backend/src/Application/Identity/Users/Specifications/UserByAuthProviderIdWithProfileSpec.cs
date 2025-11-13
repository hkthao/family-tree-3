using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Users.Specifications;

public class UserByAuthProviderIdWithProfileSpec : SingleResultSpecification<User>
{
    public UserByAuthProviderIdWithProfileSpec(string authProviderId)
    {
        Query.Where(u => u.AuthProviderId == authProviderId)
             .Include(u => u.Profile);
    }
}
