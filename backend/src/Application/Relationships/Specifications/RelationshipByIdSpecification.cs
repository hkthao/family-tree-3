using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Relationships.Specifications
{
    public class RelationshipByIdSpecification : Specification<Relationship>
    {
        public RelationshipByIdSpecification(Guid id)
        {
            Query.Where(r => r.Id == id);
        }
    }
}
