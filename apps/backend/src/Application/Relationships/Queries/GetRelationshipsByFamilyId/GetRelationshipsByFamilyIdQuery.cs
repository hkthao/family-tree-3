using backend.Application.Common.Models;

namespace backend.Application.Relationships.Queries.GetRelationshipsByFamilyId;

public record GetRelationshipsByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<RelationshipDto>>>;
