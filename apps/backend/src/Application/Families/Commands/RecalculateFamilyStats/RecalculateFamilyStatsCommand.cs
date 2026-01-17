using backend.Application.Common.Models;
using backend.Application.Families.Queries;

namespace backend.Application.Families.Commands.RecalculateFamilyStats;

/// <summary>
/// Lệnh tính toán lại tổng số thành viên và tổng số thế hệ cho một gia đình cụ thể.
/// </summary>
public record RecalculateFamilyStatsCommand : IRequest<Result<FamilyStatsDto>>
{
    /// <summary>
    /// ID của gia đình cần tính toán lại thống kê.
    /// </summary>
    public Guid FamilyId { get; init; }
}
