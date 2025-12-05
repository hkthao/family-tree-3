using backend.Application.Common.Models;

namespace backend.Application.FamilyLinkRequests.Commands.ApproveFamilyLinkRequest;

public record ApproveFamilyLinkRequestCommand(Guid RequestId, string? ResponseMessage) : IRequest<Result<Unit>>;