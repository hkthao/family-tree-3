using backend.Application.Events;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.GetEvents;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEvents([FromQuery] GetEventsQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        return Ok(await _mediator.Send(new GetEventByIdQuery(id)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateEventCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetEventById), new { id = result }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteEventCommand(id));

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<EventDto>>> Search([FromQuery] SearchEventsQuery query)
    {
        return await _mediator.Send(query);
    }
}
