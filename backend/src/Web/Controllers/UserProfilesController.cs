using backend.Application.UserProfiles.Queries.GetAllUserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
public class UserProfilesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserProfileDto>>> GetAllUserProfiles()
    {
        return await Mediator.Send(new GetAllUserProfilesQuery());
    }
}
