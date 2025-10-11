using backend.Application.Common.Models;

namespace backend.Application.Relationships.Commands.DeleteRelationship
{
    public record DeleteRelationshipCommand(Guid Id) : IRequest<Result<bool>>;
}
