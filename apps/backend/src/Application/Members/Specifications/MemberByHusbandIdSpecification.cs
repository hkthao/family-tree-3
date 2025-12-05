using backend.Domain.Entities;
using Ardalis.Specification;

namespace backend.Application.Members.Specifications;

/// <summary>
/// Đặc tả để lọc thành viên theo ID của chồng.
/// </summary>
public class MemberByHusbandIdSpecification : Specification<Member>
{
    public MemberByHusbandIdSpecification(Guid? husbandId)
    {
        if (husbandId.HasValue)
        {
            Query.Where(member => member.HusbandId == husbandId.Value);
        }
    }
}