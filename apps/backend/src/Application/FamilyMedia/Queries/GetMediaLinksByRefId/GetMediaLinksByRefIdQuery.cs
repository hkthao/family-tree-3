using backend.Application.Common.Models;
using backend.Application.FamilyMedia.DTOs;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Queries.GetMediaLinksByRefId;

public record GetMediaLinksByRefIdQuery(Guid RefId, RefType RefType, Guid FamilyId) : IRequest<Result<List<MediaLinkDto>>>;
