using backend.Application.Events;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.GetEvents;
using backend.Application.Events.Queries.SearchEvents; // Added
using backend.Application.Common.Models; // Added
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
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
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error); // Assuming NotFound for single item retrieval failure
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateEventCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetEventById), new { id = result.Value }, result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteEventCommand(id));
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return BadRequest(result.Error);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<EventDto>>> Search([FromQuery] SearchEventsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
