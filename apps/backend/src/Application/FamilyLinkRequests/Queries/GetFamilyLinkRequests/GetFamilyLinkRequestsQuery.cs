using backend.Application.Common.Models;
using backend.Application.FamilyLinks.Queries; // New using directive
using MediatR;

namespace backend.Application.FamilyLinkRequests.Queries.GetFamilyLinkRequests;

public record GetFamilyLinkRequestsQuery(Guid FamilyId) : IRequest<Result<List<FamilyLinkRequestDto>>>;