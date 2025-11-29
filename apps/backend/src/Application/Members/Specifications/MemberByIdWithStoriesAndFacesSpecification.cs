using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

public class MemberByIdWithStoriesAndFacesSpecification : Specification<Member>, ISingleResultSpecification<Member>
{
    public MemberByIdWithStoriesAndFacesSpecification(Guid memberId)
    {
        Query.Where(m => m.Id == memberId)
             .Include(m => m.MemberStories)
             .Include(m => m.MemberFaces);
    }
}
