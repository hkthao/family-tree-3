using backend.Application.UserActivities.Queries;
using backend.Application.UserActivities.Queries.GetRecentActivities;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/activities")]
public class UserActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserActivitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of recent user activities for the current user.
    /// </summary>
    /// <param name="limit">The maximum number of activities to return (default: 20).</param>
    /// <param name="targetType">Optional: Filter activities by the type of the target resource.</param>
    /// <param name="targetId">Optional: Filter activities by the ID of the target resource.</param>
    /// <param name="familyId">Optional: Filter activities by FamilyId.</param>
    /// <returns>A list of recent user activities.</returns>
    [HttpGet("recent")]
    public async Task<ActionResult<List<UserActivityDto>>> GetRecentActivities(
        [FromQuery] int limit = 20,
        [FromQuery] TargetType? targetType = null,
        [FromQuery] Guid? targetId = null,
        [FromQuery] Guid? familyId = null)
    {
        var query = new GetRecentActivitiesQuery
        {
            Limit = limit,
            TargetType = targetType,
            TargetId = targetId,
            FamilyId = familyId
        };

        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
