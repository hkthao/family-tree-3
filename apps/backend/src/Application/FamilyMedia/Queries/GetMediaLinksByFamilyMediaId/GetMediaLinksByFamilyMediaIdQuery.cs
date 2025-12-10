using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;

namespace backend.Application.FamilyMedia.Queries.GetMediaLinksByFamilyMediaId;

public record GetMediaLinksByFamilyMediaIdQuery(Guid FamilyMediaId, Guid FamilyId) : IRequest<Result<List<MediaLinkDto>>>;
