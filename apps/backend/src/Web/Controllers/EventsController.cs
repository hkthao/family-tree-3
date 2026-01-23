using backend.Application.Events.Commands.GenerateAndNotifyEvents;
using backend.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Route("api/[controller]")]
public class EventsController : BaseApiController
{
    [HttpPost("generate-and-notify")]
    public async Task<IActionResult> GenerateAndNotifyEvents([FromBody] GenerateAndNotifyEventsCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
