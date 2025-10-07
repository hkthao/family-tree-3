using backend.Application.Dashboard.Queries;
using backend.Application.Dashboard.Queries.GetDashboardStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves dashboard statistics for the current user.
    /// </summary>
    /// <param name="familyId">Optional: Filter statistics by FamilyId.</param>
    /// <returns>Dashboard statistics.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats([FromQuery] Guid? familyId = null)
    {
        var query = new GetDashboardStatsQuery { FamilyId = familyId };
        var result = await _mediator.Send(query);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
