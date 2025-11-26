using backend.Application.Common.Models;

namespace backend.Application.Relationships.Queries.GetRelationshipById;

public record GetRelationshipByIdQuery(Guid Id) : IRequest<Result<RelationshipDto>>;
