using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;

namespace backend.Application.FamilyMedia.Queries.GetFamilyMediaById;

public record GetFamilyMediaByIdQuery(Guid Id, Guid FamilyId) : IRequest<Result<FamilyMediaDto>>;
