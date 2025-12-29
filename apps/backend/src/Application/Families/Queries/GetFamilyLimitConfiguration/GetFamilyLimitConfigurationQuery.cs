using backend.Application.Common.Models;

namespace backend.Application.Families.Queries;

/// <summary>
/// Query để lấy cấu hình giới hạn của một gia đình cụ thể.
/// </summary>
public record GetFamilyLimitConfigurationQuery : IRequest<Result<FamilyLimitConfigurationDto>>
{
    /// <summary>
    /// ID của gia đình.
    /// </summary>
    public Guid FamilyId { get; init; }
}
