namespace backend.Application.FamilyTree.Queries.GetFamilyTreeJson;

public record GetFamilyTreeJsonQuery(string FamilyId) : IRequest<FamilyTreeDto>;
