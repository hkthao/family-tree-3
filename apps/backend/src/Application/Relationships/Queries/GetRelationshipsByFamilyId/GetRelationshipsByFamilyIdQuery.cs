using MediatR;
using backend.Application.Common.Models;
using backend.Application.Relationships.Queries; // Assuming RelationshipDto is here

namespace backend.Application.Relationships.Queries.GetRelationshipsByFamilyId;

public record GetRelationshipsByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<RelationshipDto>>>;
