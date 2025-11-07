using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Users.Specifications;

public class UserByProfileIdWithProfileSpec : SingleResultSpecification<User>
{
    public UserByProfileIdWithProfileSpec(Guid profileId)
    {
        Query.Where(u => u.Profile != null && u.Profile.Id == profileId)
             .Include(u => u.Profile);
    }
}
