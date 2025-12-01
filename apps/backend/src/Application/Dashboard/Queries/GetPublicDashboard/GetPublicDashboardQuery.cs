using backend.Application.Common.Models;
using MediatR;

namespace backend.Application.Dashboard.Queries.GetPublicDashboard;

public record GetPublicDashboardQuery : IRequest<Result<PublicDashboardDto>>;
