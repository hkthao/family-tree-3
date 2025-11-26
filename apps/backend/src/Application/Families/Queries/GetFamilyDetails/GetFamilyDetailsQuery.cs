using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetFamilyDetails;

public record GetFamilyDetailsQuery(Guid FamilyId) : IRequest<Result<FamilyDetailsDto>>;
