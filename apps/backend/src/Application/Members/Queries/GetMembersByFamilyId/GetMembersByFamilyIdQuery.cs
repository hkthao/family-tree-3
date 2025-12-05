using backend.Application.Common.Models;

namespace backend.Application.Members.Queries.GetMembersByFamilyId;

/// <summary>
/// Truy vấn để lấy danh sách các thành viên thuộc một gia đình cụ thể.
/// </summary>
/// <param name="FamilyId">ID của gia đình.</param>
public record GetMembersByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<MemberDto>>>;