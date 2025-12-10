using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedia.Commands.LinkMediaToEntity;

public record LinkMediaToEntityCommand : IRequest<Result<Guid>>
{
    public Guid FamilyMediaId { get; init; }
    public RefType RefType { get; init; }
    public Guid RefId { get; init; }
}
