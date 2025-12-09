using backend.Application.Services;

namespace backend.Application.Relationships.Queries.GetRelationship;

public record GetRelationshipQuery(Guid FamilyId, Guid MemberAId, Guid MemberBId) : IRequest<RelationshipDetectionResult>;
