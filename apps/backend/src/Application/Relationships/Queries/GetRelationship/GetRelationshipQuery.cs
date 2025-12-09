using backend.Application.Common.Interfaces;

namespace backend.Application.Relationships.Queries.GetRelationship;

public record GetRelationshipQuery(Guid FamilyId, Guid MemberAId, Guid MemberBId) : IRequest<RelationshipDetectionResult>;
