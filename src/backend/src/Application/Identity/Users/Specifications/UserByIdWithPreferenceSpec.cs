using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Users.Specifications;

public class UserByIdWithPreferenceSpec : SingleResultSpecification<User>
{
    public UserByIdWithPreferenceSpec(Guid userId)
    {
        Query.Where(u => u.Id == userId)
             .Include(u => u.Preference);
    }
}
