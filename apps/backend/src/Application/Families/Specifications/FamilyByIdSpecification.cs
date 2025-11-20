using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications;

public class FamilyByIdSpecification : Specification<Family>, ISingleResultSpecification<Family>
{
    public FamilyByIdSpecification(Guid id)
    {
        Query.Where(f => f.Id == id)
             .Include(f => f.FamilyUsers)
                 .ThenInclude(fu => fu.User)
                     .ThenInclude(u => u.Profile);
    }
}
