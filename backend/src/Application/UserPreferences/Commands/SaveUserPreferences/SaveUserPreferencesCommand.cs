using backend.Application.Common.Models;
using backend.Domain.Enums;

namespace backend.Application.UserPreferences.Commands.SaveUserPreferences;

public class SaveUserPreferencesCommand : IRequest<Result>
{
    public Theme Theme { get; set; }
    public Language Language { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool SmsNotificationsEnabled { get; set; }
    public bool InAppNotificationsEnabled { get; set; }
}
