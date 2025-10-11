using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Families.Specifications
{
    public class FamilyPaginationSpecification : Specification<Family>
    {
        public FamilyPaginationSpecification(int skip, int take)
        {
            Query.Skip(skip).Take(take);
        }
    }
}
