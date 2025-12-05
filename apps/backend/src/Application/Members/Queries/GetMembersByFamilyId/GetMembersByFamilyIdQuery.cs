using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.GetMembersByFamilyId;

/// <summary>
/// Truy vấn để lấy danh sách các thành viên thuộc một gia đình cụ thể.
/// </summary>
/// <param name="FamilyId">ID của gia đình.</param>
public record GetMembersByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<MemberDto>>>;