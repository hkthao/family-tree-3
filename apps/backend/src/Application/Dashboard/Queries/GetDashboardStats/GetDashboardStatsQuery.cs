using backend.Application.Common.Models;
using backend.Application.Dashboard; // Added using statement for DashboardStatsDto

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>
{
    public Guid? FamilyId { get; init; }
}
