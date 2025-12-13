using backend.Application.Common.Models;
using backend.Application.FamilyMedias.DTOs;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedias.Queries.GetMediaLinksByRefId;

public record GetMediaLinksByRefIdQuery(Guid RefId, RefType RefType, Guid FamilyId) : IRequest<Result<List<MediaLinkDto>>>;
