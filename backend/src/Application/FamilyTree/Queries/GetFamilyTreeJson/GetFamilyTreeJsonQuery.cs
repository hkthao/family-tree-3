namespace backend.Application.FamilyTree.Queries.GetFamilyTreeJson;

public record GetFamilyTreeJsonQuery(Guid FamilyId) : IRequest<FamilyTreeDto>;
