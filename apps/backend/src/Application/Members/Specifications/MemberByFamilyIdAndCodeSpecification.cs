using Ardalis.Specification;
using backend.Domain.Entities;

namespace backend.Application.Members.Specifications;

/// <summary>
/// Đặc tả để lấy một thành viên dựa trên ID gia đình và mã thành viên.
/// </summary>
public class MemberByFamilyIdAndCodeSpecification : Specification<Member>, ISingleResultSpecification<Member>
{
    public MemberByFamilyIdAndCodeSpecification(Guid familyId, string memberCode)
    {
        Query.Where(m => m.FamilyId == familyId && m.Code == memberCode);
    }
}
