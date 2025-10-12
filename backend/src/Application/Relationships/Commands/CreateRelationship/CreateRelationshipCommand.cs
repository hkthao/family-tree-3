using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.Inputs;

namespace backend.Application.Relationships.Commands.CreateRelationship;

public record CreateRelationshipCommand : RelationshipInput, IRequest<Result<Guid>>
{
}
