using backend.Application.Common.Models;
using backend.Application.Families.Queries.GetFamilyById;

namespace backend.Application.Families.Queries.GetPublicFamilyById;

/// <summary>
/// Truy vấn để lấy thông tin chi tiết của một gia đình công khai theo ID.
/// </summary>
public record GetPublicFamilyByIdQuery(Guid Id) : IRequest<Result<FamilyDetailDto>>;
