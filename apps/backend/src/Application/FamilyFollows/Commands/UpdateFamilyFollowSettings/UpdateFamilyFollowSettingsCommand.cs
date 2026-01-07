using backend.Application.Common.Models;

namespace backend.Application.FamilyFollows.Commands.UpdateFamilyFollowSettings;

public record UpdateFamilyFollowSettingsCommand : IRequest<Result>
{
    public Guid FamilyId { get; init; }
    public bool NotifyDeathAnniversary { get; init; }
    public bool NotifyBirthday { get; init; }
    public bool NotifyEvent { get; init; }
}
