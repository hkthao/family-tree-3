using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileById; // New using directive
using backend.Application.Notifications.DTOs;
using Microsoft.Extensions.Logging;

namespace backend.Application.Notifications.Commands.SyncSubscriber;

public class SyncSubscriberCommandHandler(INotificationService notificationService, ILogger<SyncSubscriberCommandHandler> logger, IMediator mediator) : IRequestHandler<SyncSubscriberCommand, Result>
{
    private readonly INotificationService _notificationService = notificationService;
    private readonly ILogger<SyncSubscriberCommandHandler> _logger = logger;
    private readonly IMediator _mediator = mediator;

    public async Task<Result> Handle(SyncSubscriberCommand request, CancellationToken cancellationToken)
    {
        UserProfileDto? userProfile = null;

        // Fetch user profile from the database
        var userProfileQueryResult = await _mediator.Send(new GetUserProfileByIdQuery { Id = request.UserId }, cancellationToken);
        if (userProfileQueryResult.IsSuccess && userProfileQueryResult.Value != null)
        {
            userProfile = userProfileQueryResult.Value;
        }

        var subscriberDto = new SyncSubscriberDto
        {
            UserId = request.UserId.ToString(),
            FirstName = userProfile?.FirstName,
            LastName = userProfile?.LastName,
            Email = userProfile?.Email,
            Phone = userProfile?.Phone,
            Avatar = userProfile?.Avatar,
            Locale = "vi-VN", // Assuming locale is in preferences
            Timezone = "Asia/Ho_Chi_Minh", // Assuming timezone is in preferences
        };

        var syncResult = await _notificationService.SyncSubscriberAsync(subscriberDto, cancellationToken);
        if (!syncResult.IsSuccess)
        {
            _logger.LogError("Failed to sync subscriber {UserId} with notification service: {Errors}", request.UserId, syncResult.Error);
            return Result.Failure(syncResult.Error ?? "Unknown error during subscriber sync", syncResult.ErrorSource ?? "Unknown"); // Explicitly return a non-nullable Result.Failure
        }

        return Result.Success();
    }
}
