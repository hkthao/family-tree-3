using backend.Application.Common.Models;

namespace backend.Application.FamilyDicts.Queries;

public record GetFamilyDictByIdQuery(Guid Id) : IRequest<Result<FamilyDictDto>>;
