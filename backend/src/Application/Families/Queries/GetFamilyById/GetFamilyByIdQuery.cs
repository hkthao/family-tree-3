using backend.Application.Common.Models; // Added

namespace backend.Application.Families.Queries.GetFamilyById;

public record GetFamilyByIdQuery(Guid Id) : IRequest<Result<FamilyDetailDto>>;
