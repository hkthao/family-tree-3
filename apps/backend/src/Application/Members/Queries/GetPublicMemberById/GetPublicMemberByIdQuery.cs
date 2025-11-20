using backend.Application.Common.Models;
using backend.Application.Members.Queries.GetMemberById;

namespace backend.Application.Members.Queries.GetPublicMemberById;

/// <summary>
/// Truy vấn để lấy thông tin chi tiết của một thành viên cụ thể trong một gia đình công khai.
/// </summary>
public record GetPublicMemberByIdQuery(Guid Id, Guid FamilyId) : IRequest<Result<MemberDetailDto>>;
