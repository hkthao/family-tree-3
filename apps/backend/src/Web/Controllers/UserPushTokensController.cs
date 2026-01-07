using backend.Application.Common.Constants;
using backend.Application.UserPushTokens.Commands.CreateUserPushToken;
using backend.Application.UserPushTokens.Commands.DeleteUserPushToken;
using backend.Application.UserPushTokens.Commands.UpdateUserPushToken;
using backend.Application.UserPushTokens.Queries.GetUserPushTokenById;
using backend.Application.UserPushTokens.Queries.GetUserPushTokensByUserId;
using backend.Application.UserPushTokens.Queries.SearchUserPushTokens; // New using directive
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/user-push-tokens")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class UserPushTokensController(IMediator mediator, ILogger<UserPushTokensController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<UserPushTokensController> _logger = logger;

    [HttpPost]
    public async Task<IActionResult> CreateUserPushToken(CreateUserPushTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetUserPushTokenById), new { id = result.Value });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserPushToken(Guid id, UpdateUserPushTokenCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateUserPushTokenCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserPushToken(Guid id)
    {
        var result = await _mediator.Send(new DeleteUserPushTokenCommand(id));
        return result.ToActionResult(this, _logger, 204);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserPushTokenById(Guid id)
    {
        var result = await _mediator.Send(new GetUserPushTokenByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPushTokensByUserId(Guid userId)
    {
        var result = await _mediator.Send(new GetUserPushTokensByUserIdQuery(userId));
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUserPushTokens([FromQuery] SearchUserPushTokensQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }
}

