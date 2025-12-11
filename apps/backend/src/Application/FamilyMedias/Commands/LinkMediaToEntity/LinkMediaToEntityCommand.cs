using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.FamilyMedias.Commands.LinkMediaToEntity;

public record LinkMediaToEntityCommand : IRequest<Result<Guid>>
{
    public Guid FamilyMediaId { get; init; }
    public RefType RefType { get; init; }
    public Guid RefId { get; init; }
}
