using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMemberById;

namespace backend.Application.Members.Queries.GetMemberByFamilyIdAndCode;

/// <summary>
/// Truy vấn để lấy thông tin chi tiết của một thành viên dựa trên ID gia đình và mã thành viên.
/// </summary>
public record GetMemberByFamilyIdAndCodeQuery(Guid FamilyId, string MemberCode) : IRequest<Result<MemberDetailDto>>;
