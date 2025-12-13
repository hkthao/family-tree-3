using backend.Application.Common.Interfaces;

namespace backend.Application.Relationships.Queries.DetectRelationship;

public record DetectRelationshipQuery(Guid FamilyId, Guid MemberAId, Guid MemberBId) : IRequest<RelationshipDetectionResult>;
