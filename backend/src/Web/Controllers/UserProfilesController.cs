using backend.Application.UserProfiles.Queries;
using backend.Application.UserProfiles.Queries.GetAllUserProfiles;
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
}
