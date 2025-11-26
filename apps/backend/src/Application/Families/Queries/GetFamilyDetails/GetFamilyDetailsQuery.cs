using backend.Application.Common.Models;
using backend.Application.Families; // Added for FamilyDetailsDto

namespace backend.Application.Families.Queries.GetFamilyDetails;

public record GetFamilyDetailsQuery(Guid FamilyId) : IRequest<Result<FamilyDetailsDto>>;
