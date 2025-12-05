using backend.Domain.Entities;
using Ardalis.Specification;

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