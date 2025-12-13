using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;

namespace backend.Application.FamilyMedias.Queries.GetMediaLinksByFamilyMediaId;

public record GetMediaLinksByFamilyMediaIdQuery(Guid FamilyMediaId, Guid FamilyId) : IRequest<Result<List<MediaLinkDto>>>;
