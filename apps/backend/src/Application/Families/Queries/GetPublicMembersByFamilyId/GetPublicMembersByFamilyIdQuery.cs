using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMembers;

namespace backend.Application.Members.Queries.GetPublicMembersByFamilyId;

/// <summary>
/// Truy vấn để lấy danh sách các thành viên của một gia đình công khai theo Family ID.
/// </summary>
public record GetPublicMembersByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<MemberListDto>>>;
