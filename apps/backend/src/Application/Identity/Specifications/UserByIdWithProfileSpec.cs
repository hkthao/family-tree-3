using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Users.Specifications;

public class UserByIdWithProfileSpec : SingleResultSpecification<User>
{
    public UserByIdWithProfileSpec(Guid userId)
    {
        Query.Where(u => u.Id == userId)
             .Include(u => u.Profile);
    }
}
