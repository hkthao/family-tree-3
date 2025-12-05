using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.DeleteLinkFamily;

public record DeleteLinkFamilyCommand(Guid FamilyLinkId) : IRequest<Result<Unit>>;