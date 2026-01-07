using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Commands.UnfollowFamily;

public record UnfollowFamilyCommand : IRequest<Result>
{
    public Guid FamilyId { get; init; }
}
