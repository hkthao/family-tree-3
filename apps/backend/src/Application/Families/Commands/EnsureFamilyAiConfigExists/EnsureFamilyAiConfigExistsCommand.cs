using MediatR;
using backend.Application.Common.Models;

namespace backend.Application.Families.Commands.EnsureFamilyAiConfigExists;

/// <summary>
/// Lệnh để đảm bảo cấu hình giới hạn AI cho gia đình tồn tại.
/// </summary>
public record EnsureFamilyAiConfigExistsCommand : IRequest<Result>
{
    /// <summary>
    /// ID của gia đình.
    /// </summary>
    public Guid FamilyId { get; init; }
}
