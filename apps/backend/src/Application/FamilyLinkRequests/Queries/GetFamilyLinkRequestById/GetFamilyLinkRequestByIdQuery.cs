using backend.Application.Common.Models;
using backend.Application.FamilyLinks.Queries; // New using directive
using MediatR;

namespace backend.Application.FamilyLinkRequests.Queries.GetFamilyLinkRequestById;

public record GetFamilyLinkRequestByIdQuery(Guid Id) : IRequest<Result<FamilyLinkRequestDto>>;

