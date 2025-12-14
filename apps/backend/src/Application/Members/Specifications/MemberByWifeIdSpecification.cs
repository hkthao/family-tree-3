using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

/// <summary>
/// Đặc tả để lọc thành viên theo ID của vợ.
/// </summary>
public class MemberByWifeIdSpecification : Specification<Member>
{
    public MemberByWifeIdSpecification(Guid? wifeId)
    {
        if (wifeId.HasValue)
        {
            Query.Where(member => member.WifeId == wifeId.Value);
        }
    }
}
