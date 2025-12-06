using backend.Application.Common.Models;

namespace backend.Application.Dashboard.Queries.GetPublicDashboard;

public record GetPublicDashboardQuery(Guid FamilyId) : IRequest<Result<PublicDashboardDto>>;
