using backend.Application.Common.Models;
using backend.Application.MemoryItems.Commands.CreateMemoryItem;
using backend.Application.MemoryItems.Commands.DeleteMemoryItem;
using backend.Application.MemoryItems.Commands.UpdateMemoryItem;
using backend.Application.MemoryItems.DTOs;
using backend.Application.MemoryItems.Queries.GetMemoryItemDetail;
using backend.Application.MemoryItems.Queries.SearchMemoryItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController] // Add this
[Route("api/memory-items")] // Add this
public class MemoryItemsController : ControllerBase
{
    private readonly IMediator _mediator; // Add this
    private readonly ILogger<MemoryItemsController> _logger; // Add this

    public MemoryItemsController(IMediator mediator, ILogger<MemoryItemsController> logger) // Add constructor
    {
        _mediator = mediator;
        _logger = logger;
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateMemoryItem(CreateMemoryItemCommand command)
    {
        var familyIdString = HttpContext.GetRouteValue("familyId")?.ToString();
        if (!Guid.TryParse(familyIdString, out var familyId))
        {
            return BadRequest(Result<Guid>.Failure("Invalid Family ID in route."));
        }
        command.FamilyId = familyId;
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemoryItem(Guid id, UpdateMemoryItemCommand command)
    {
        var familyIdString = HttpContext.GetRouteValue("familyId")?.ToString();
        if (!Guid.TryParse(familyIdString, out var familyId))
        {
            return BadRequest(Result.Failure("Invalid Family ID in route."));
        }

        if (id != command.Id)
        {
            return BadRequest(Result.Failure("Memory Item ID in route must match Memory Item ID in body."));
        }
        command.FamilyId = familyId; // Assign FamilyId from route
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMemoryItem(Guid id)
    {
        var familyIdString = HttpContext.GetRouteValue("familyId")?.ToString();
        if (!Guid.TryParse(familyIdString, out var familyId))
        {
            return BadRequest(Result.Failure("Invalid Family ID in route."));
        }
        var result = await _mediator.Send(new DeleteMemoryItemCommand { Id = id, FamilyId = familyId });
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemoryItemDetail(Guid id)
    {
        var familyIdString = HttpContext.GetRouteValue("familyId")?.ToString();
        if (!Guid.TryParse(familyIdString, out var familyId))
        {
            return BadRequest(Result.Failure("Invalid Family ID in route."));
        }
        var result = await _mediator.Send(new GetMemoryItemDetailQuery { Id = id, FamilyId = familyId });
        return result.ToActionResult(this, _logger);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<MemoryItemDto>>> SearchMemoryItems([FromQuery] SearchMemoryItemsQuery query)
    {
        var familyIdString = HttpContext.GetRouteValue("familyId")?.ToString();
        if (!Guid.TryParse(familyIdString, out var familyId))
        {
            return BadRequest(Result<PaginatedList<MemoryItemDto>>.Failure("Invalid Family ID in route."));
        }
        query.FamilyId = familyId; // Assign FamilyId from route
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
