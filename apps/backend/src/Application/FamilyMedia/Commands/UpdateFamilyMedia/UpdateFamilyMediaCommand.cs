using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Commands.UpdateFamilyMedia;

public record UpdateFamilyMediaCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid FamilyId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public MediaType MediaType { get; init; }
    public string? Description { get; init; }
}
