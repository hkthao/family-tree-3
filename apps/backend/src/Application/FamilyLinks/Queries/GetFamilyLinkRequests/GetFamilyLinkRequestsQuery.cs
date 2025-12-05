using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Queries.GetFamilyLinkRequests;

public record GetFamilyLinkRequestsQuery(Guid FamilyId) : IRequest<Result<List<FamilyLinkRequestDto>>>;