using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.ApproveFamilyLinkRequest;

public record ApproveFamilyLinkRequestCommand(Guid RequestId) : IRequest<Result<Unit>>;