using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Notifications.Commands.SyncSubscriber;
using backend.Application.UserPushTokens.Commands.SyncCurrentUserPushToken; // New using directive
using backend.Application.UserPushTokens.Commands.RemoveCurrentUserPushToken; // Changed using directive
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController] // Add ApiController attribute for API conventions
[Route("api/notification")] // Add route attribute
public class NotificationsController : ControllerBase
{
    private readonly ILogger<NotificationsController> _logger;
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser; // Inject ICurrentUser

    public NotificationsController(ILogger<NotificationsController> logger, IMediator mediator, ICurrentUser currentUser)
    {
        _logger = logger;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost("push-token")]
    public async Task<ActionResult<Result>> SyncPushToken([FromBody] SyncCurrentUserPushTokenCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result.Error);
        return Ok(Result.Success());
    }

    [HttpDelete("push-token")]
    public async Task<ActionResult<Result>> RemovePushToken([FromQuery] string deviceId, [FromQuery] string expoPushToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Unauthorized(Result.Failure("User is not authenticated.", "Authentication"));
        }

        // Command validator will handle validation for deviceId and expoPushToken
        var command = new RemoveCurrentUserPushTokenCommand(deviceId, expoPushToken);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(Result.Success());
    }

    [HttpPost("subscriber")]
    public async Task<ActionResult<Result>> SyncSubscriber()
    {
        if (!_currentUser.IsAuthenticated)
            return Unauthorized(Result.Failure("User is not authenticated.", "Authentication"));
        var command = new SyncSubscriberCommand(_currentUser.UserId);
        var result = await _mediator.Send(command);
        return !result.IsSuccess ? (ActionResult<Result>)BadRequest(result.Error) : (ActionResult<Result>)Ok(Result.Success());
    }
}
