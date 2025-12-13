using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;

namespace backend.Application.FamilyMedias.Queries.GetFamilyMediaById;

public record GetFamilyMediaByIdQuery(Guid Id, Guid FamilyId) : IRequest<Result<FamilyMediaDto>>;
