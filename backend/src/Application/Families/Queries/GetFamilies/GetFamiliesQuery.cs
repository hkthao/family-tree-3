using backend.Application.Common.Models;

namespace backend.Application.Families.Queries.GetFamilies;

public record GetFamiliesQuery : IRequest<List<FamilyDto>>;