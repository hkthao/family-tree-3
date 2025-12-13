using backend.Application.Common.Models;

namespace backend.Application.FamilyMedias.Commands.DeleteFamilyMedia;

public record DeleteFamilyMediaCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public Guid FamilyId { get; init; }
}
