using backend.Application.Common.Models;
using backend.Application.Relationships.Queries;

namespace backend.Application.Relationships.Queries.GetPublicRelationshipsByFamilyId;

public record GetPublicRelationshipsByFamilyIdQuery(Guid FamilyId) : IRequest<Result<List<RelationshipListDto>>>;
