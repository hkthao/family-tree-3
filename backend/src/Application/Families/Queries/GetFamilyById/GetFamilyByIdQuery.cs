namespace backend.Application.Families.Queries.GetFamilyById;

public record GetFamilyByIdQuery(Guid Id) : IRequest<FamilyDetailDto>;