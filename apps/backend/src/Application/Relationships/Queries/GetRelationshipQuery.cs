using backend.Application.Common.Interfaces;

namespace backend.Application.Relationships.Queries;

public record GetRelationshipQuery(Guid FamilyId, Guid MemberAId, Guid MemberBId) : IRequest<RelationshipDetectionResult>;
