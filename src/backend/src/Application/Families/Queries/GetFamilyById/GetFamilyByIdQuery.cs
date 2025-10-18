using backend.Application.Common.Models; 

namespace backend.Application.Families.Queries.GetFamilyById;

public record GetFamilyByIdQuery(Guid Id) : IRequest<Result<FamilyDetailDto>>;
