using backend.Application.Common.Models;

namespace backend.Application.FamilyLinkRequests.Commands.RejectFamilyLinkRequest;

public record RejectFamilyLinkRequestCommand(Guid RequestId, string? ResponseMessage) : IRequest<Result<Unit>>;