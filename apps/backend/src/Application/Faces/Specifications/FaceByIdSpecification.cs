using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Faces.Specifications;

public class FaceByIdSpecification : Specification<Face>, ISingleResultSpecification<Face>
{
    public FaceByIdSpecification(Guid id)
    {
        Query.Where(f => f.Id == id);
    }
}
