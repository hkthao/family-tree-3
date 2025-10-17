using backend.Application.Common.Models;
using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Application.UserPreferences.Queries;
using backend.Application.UserPreferences.Queries.GetUserPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserPreferencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserPreferencesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the current user's preferences.
    /// </summary>
    /// <returns>The user's preferences.</returns>
    [HttpGet]
    public async Task<ActionResult<UserPreferenceDto>> GetUserPreferences()
    {
        var result = await _mediator.Send(new GetUserPreferencesQuery());
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Saves the current user's preferences.
    /// </summary>
    /// <param name="command">The command containing user preferences data.</param>
    /// <returns>A Result indicating success or failure.</returns>
    [HttpPut]
    public async Task<ActionResult<Result>> SaveUserPreferences([FromBody] SaveUserPreferencesCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
