using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.CreateFamilyLinkRequest;

public record CreateFamilyLinkRequestCommand(Guid RequestingFamilyId, Guid TargetFamilyId) : IRequest<Result<Guid>>;