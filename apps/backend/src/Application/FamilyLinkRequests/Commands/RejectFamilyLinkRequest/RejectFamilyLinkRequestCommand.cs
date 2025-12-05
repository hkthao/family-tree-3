using backend.Application.Common.Models;

namespace backend.Application.FamilyLinkRequests.Commands.RejectFamilyLinkRequest;

public record RejectFamilyLinkRequestCommand(Guid RequestId) : IRequest<Result<Unit>>;