using backend.Application.Common.Interfaces;
using backend.Application.Events;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.Events.Queries.GetEvents;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEvents([FromQuery] GetEventsQuery query)
    {
        var result = await _eventService.GetAllAsync();
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEventById(Guid id)
    {
        var result = await _eventService.GetByIdAsync(id);
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound();
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateEventCommand command)
    {
        var anEvent = new Event
        {
            Name = command.Name,
            Description = command.Description,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Location = command.Location,
            FamilyId = command.FamilyId,
            Type = command.Type,
            Color = command.Color,
        };
        var result = await _eventService.CreateAsync(anEvent);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetEventById), new { id = result.Value }, result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var anEvent = new Event
        {
            Id = command.Id,
            Name = command.Name,
            Description = command.Description,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Location = command.Location,
            FamilyId = command.FamilyId,
            Type = command.Type,
            Color = command.Color,
        };

        var result = await _eventService.UpdateAsync(anEvent);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _eventService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }
}
