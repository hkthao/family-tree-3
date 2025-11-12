using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Faces.Specifications;

public class FacesByMemberIdSpecification : Specification<Face>
{
    public FacesByMemberIdSpecification(Guid memberId)
    {
        Query.Where(f => f.MemberId == memberId);
    }
}
