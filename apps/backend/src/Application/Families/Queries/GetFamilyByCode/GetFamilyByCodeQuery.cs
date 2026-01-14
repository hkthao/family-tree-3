using backend.Application.Common.Models;

namespace backend.Application.Families.Queries;

public record GetFamilyByCodeQuery(string Code) : IRequest<Result<FamilyDto>>;
