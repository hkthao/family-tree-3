using backend.Application.Identity.UserProfiles.Commands.SyncNotificationSubscriber;
using backend.Application.Identity.UserProfiles.Commands.SyncUserProfile;
using backend.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace backend.Application.Identity.UserProfiles.EventHandlers;

/// <summary>
/// Xử lý sự kiện UserLoggedInEvent để đồng bộ hóa hồ sơ người dùng.
/// </summary>
public class UserLoggedInEventHandler : INotificationHandler<UserLoggedInEvent>
{
    private readonly ILogger<UserLoggedInEventHandler> _logger;
    private readonly IMediator _mediator;

    /// <summary>
    /// Khởi tạo một phiên bản mới của UserLoggedInEventHandler.
    /// </summary>
    /// <param name="logger">Logger để ghi nhật ký thông tin.</param>
    /// <param name="mediator">Mediator để gửi các lệnh và truy vấn.</param>
    public UserLoggedInEventHandler(ILogger<UserLoggedInEventHandler> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Xử lý UserLoggedInEvent bằng cách gửi SyncUserProfileCommand.
    /// </summary>
    /// <param name="notification">Sự kiện UserLoggedInEvent.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Task biểu thị hoạt động không đồng bộ.</returns>
    public async Task Handle(UserLoggedInEvent notification, CancellationToken cancellationToken)
    {
        foreach (var claim in notification.UserPrincipal.Claims)
        {
            _logger.LogInformation("UserLoggedInEvent Claim {Type}: {Value}", claim.Type, claim.Value);
        }

        var command = new SyncUserProfileCommand
        {
            UserPrincipal = notification.UserPrincipal
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogError("Error syncing user profile for external ID: {ExternalId}. Details: {Error}", notification.UserPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, result.Error);
        }

        var syncNotificationSubscriberCommand = new SyncNotificationSubscriberCommand()
        {
            UserProfile = result.Value!
        };
        var syncResult = await _mediator.Send(syncNotificationSubscriberCommand, cancellationToken);
        if (!syncResult.IsSuccess)
        {
            _logger.LogError("Error sync notification subscriber for profile ID: {Id}. Details: {Error}", result.Value?.Id, syncResult.Error);
        }

    }
}
