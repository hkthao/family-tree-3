using backend.Application.Common.Models;

namespace backend.Application.FamilyLinkRequests.Commands.CreateFamilyLinkRequest;

public record CreateFamilyLinkRequestCommand(Guid RequestingFamilyId, Guid TargetFamilyId, string? RequestMessage) : IRequest<Result<Guid>>;