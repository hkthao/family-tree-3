using backend.Application.Common.Models;

namespace backend.Application.FamilyLinks.Commands.UnlinkFamilies;

public record UnlinkFamiliesCommand(Guid Family1Id, Guid Family2Id) : IRequest<Result<Unit>>;