namespace backend.Application.Families.Queries.GetFamiliesByIds;

public record GetFamiliesByIdsQuery(List<Guid> Ids) : IRequest<Result<List<FamilyDto>>>;
