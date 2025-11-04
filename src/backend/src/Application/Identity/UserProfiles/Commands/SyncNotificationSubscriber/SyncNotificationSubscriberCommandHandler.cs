using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;

/// <summary>
/// Xử lý lệnh SyncNotificationSubscriberCommand để đồng bộ hóa thông tin người dùng với dịch vụ thông báo.
/// </summary>
public class SyncNotificationSubscriberCommandHandler(
    ILogger<SyncNotificationSubscriberCommandHandler> logger,
    INotificationProviderFactory notificationProviderFactory,
    IOptions<NotificationSettings> notificationSettings) : IRequestHandler<SyncNotificationSubscriberCommand, Result<bool>>
{
    private readonly ILogger<SyncNotificationSubscriberCommandHandler> _logger = logger;
    private readonly INotificationProviderFactory _notificationProviderFactory = notificationProviderFactory;
    private readonly NotificationSettings _notificationSettings = notificationSettings.Value;

    public async Task<Result<bool>> Handle(SyncNotificationSubscriberCommand request, CancellationToken cancellationToken)
    {
        var id = request.UserProfile.Id.ToString();
        var email = request.UserProfile.Email;
        var firstName = request.UserProfile.FirstName;
        var lastName = request.UserProfile.LastName;
        var phone = request.UserProfile.Phone;

        if (string.IsNullOrEmpty(_notificationSettings.Provider))
        {
            _logger.LogWarning("Notification provider is not configured. Skipping subscriber synchronization.");
            return Result<bool>.Success(false); // Indicate that no action was taken
        }

        try
        {
            var provider = _notificationProviderFactory.GetProvider(_notificationSettings.Provider);
            await provider.SyncSubscriberAsync(
                id,
                firstName,
                lastName,
                email,
                phone);

            _logger.LogInformation("Successfully synchronized notification subscriber for external ID: {id}.", id);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing notification subscriber for external ID: {id}. Details: {Error}", id, ex.Message);
            return Result<bool>.Failure(string.Format(ErrorMessages.UnexpectedError, ex.Message), ErrorSources.Exception);
        }
    }
}
