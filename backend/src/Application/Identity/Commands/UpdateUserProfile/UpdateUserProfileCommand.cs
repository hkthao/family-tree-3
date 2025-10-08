using backend.Application.Common.Models;

namespace backend.Application.Identity.Commands.UpdateUserProfile;

public class UpdateUserProfileCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, object>? UserMetadata { get; set; }

    public bool? EmailNotificationsEnabled { get; set; }
    public bool? SmsNotificationsEnabled { get; set; }
    public bool? InAppNotificationsEnabled { get; set; }
}