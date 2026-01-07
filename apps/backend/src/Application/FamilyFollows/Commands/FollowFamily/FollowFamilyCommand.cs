using backend.Application.Common.Interfaces;
using MediatR;
using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Commands.FollowFamily;

public record FollowFamilyCommand : IRequest<Result<Guid>> // Changed return type to Result<Guid>
{
    public Guid FamilyId { get; init; }
    public bool NotifyDeathAnniversary { get; init; } = true;
    public bool NotifyBirthday { get; init; } = true;
    public bool NotifyEvent { get; init; } = true;
}
