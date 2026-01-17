using backend.Application.Common.Models;

namespace backend.Application.Dashboard.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>
{
    public Guid FamilyId { get; init; }
}
