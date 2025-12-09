using System;
using backend.Application.Services;
using MediatR;

namespace backend.Application.Relationships.Queries.GetRelationship;

public record GetRelationshipQuery(Guid FamilyId, Guid MemberAId, Guid MemberBId) : IRequest<RelationshipDetectionResult>;
