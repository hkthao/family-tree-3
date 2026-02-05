using backend.Application.Common.Models; // Add this using statement

namespace backend.Application.FamilyDicts.Queries.Public;

public record GetPublicFamilyDictByIdQuery(Guid Id) : IRequest<Result<FamilyDictDto>>;
