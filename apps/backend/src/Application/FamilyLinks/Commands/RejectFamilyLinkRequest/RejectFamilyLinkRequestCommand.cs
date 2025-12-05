using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.RejectFamilyLinkRequest;

public record RejectFamilyLinkRequestCommand(Guid RequestId) : IRequest<Result<Unit>>;