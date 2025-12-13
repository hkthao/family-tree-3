using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

/// <summary>
/// Đặc tả để lọc thành viên theo ID của cha.
/// </summary>
public class MemberByFatherIdSpecification : Specification<Member>
{
    public MemberByFatherIdSpecification(Guid? fatherId)
    {
        if (fatherId.HasValue)
        {
            Query.Where(member => member.FatherId == fatherId.Value);
        }
    }
}
