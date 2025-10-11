using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.Inputs;

namespace backend.Application.Relationships.Commands.UpdateRelationship
{
    public record UpdateRelationshipCommand : RelationshipInput, IRequest<Result<bool>>
    {
        public Guid Id { get; init; }
    }
}
