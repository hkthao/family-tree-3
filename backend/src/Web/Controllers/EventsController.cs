using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.Events.Queries.GetEvents;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<EventDto>> GetEvents([FromQuery] GetEventsQuery query)
    {
        return await Mediator.Send(query);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateEventCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteEventCommand(id));

        return NoContent();
    }
}
