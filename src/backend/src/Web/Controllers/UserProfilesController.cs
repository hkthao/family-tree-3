using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetAllUserProfiles;
using backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileByExternalId;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserProfilesController : ControllerBase
{

    private readonly IMediator _mediator;

    public UserProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
    {
        var result = await _mediator.Send(new GetCurrentUserProfileQuery());
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }

    [HttpGet]
    public async Task<ActionResult<List<UserProfileDto>>> GetAllUserProfiles()
    {
        var result = await _mediator.Send(new GetAllUserProfilesQuery());
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpGet("byExternalId/{externalId}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfileByExternalId(string externalId)
    {
        var result = await _mediator.Send(new GetUserProfileByExternalIdQuery { ExternalId = externalId });
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfileById(Guid id)
    {
        var result = await _mediator.Send(new GetUserProfileByIdQuery { Id = id });
        if (result.IsSuccess)
            return Ok(result.Value);
        return NotFound(result.Error);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="command">The command containing updated user profile data.</param>
    /// <returns>A Result indicating success or failure.</returns>
    [HttpPut("{userId}")]
    public async Task<ActionResult<Result>> UpdateUserProfile(string userId, [FromBody] UpdateUserProfileCommand command)
    {
        if (userId != command.Id)
        {
            return BadRequest(Result.Failure("User ID in URL must match user ID in request body.", "BadRequest"));
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
