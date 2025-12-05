using backend.Domain.Entities;
using Ardalis.Specification;

namespace backend.Application.Members.Specifications;

/// <summary>
/// Đặc tả để lọc thành viên theo ID của mẹ.
/// </summary>
public class MemberByMotherIdSpecification : Specification<Member>
{
    public MemberByMotherIdSpecification(Guid? motherId)
    {
        if (motherId.HasValue)
        {
            Query.Where(member => member.MotherId == motherId.Value);
        }
    }
}