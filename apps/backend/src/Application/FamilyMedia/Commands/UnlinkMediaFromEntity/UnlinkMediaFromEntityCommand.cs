using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Commands.UnlinkMediaFromEntity;

public record UnlinkMediaFromEntityCommand : IRequest<Result>
{
    public Guid FamilyMediaId { get; init; }
    public RefType RefType { get; init; }
    public Guid RefId { get; init; }
}
