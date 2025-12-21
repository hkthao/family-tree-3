using backend.Application.Common.Models;
using backend.Application.MemoryItems.Commands.CreateMemoryItem;
using backend.Application.MemoryItems.Commands.DeleteMemoryItem;
using backend.Application.MemoryItems.Commands.UpdateMemoryItem;
using backend.Application.MemoryItems.DTOs;
using backend.Application.MemoryItems.Queries.GetMemoryItemDetail;
using backend.Application.MemoryItems.Queries.SearchMemoryItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[Authorize]
[ApiController] // Add this
[Route("api/memory-items")] // Add this
[EnableRateLimiting(RateLimitConstants.UserPolicy)]
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
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMemoryItem(Guid id, UpdateMemoryItemCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateMemoryItemCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMemoryItem(Guid id)
    {
        var result = await _mediator.Send(new DeleteMemoryItemCommand { Id = id });
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemoryItemDetail(Guid id)
    {
        var result = await _mediator.Send(new GetMemoryItemDetailQuery { Id = id });
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<MemoryItemDto>>> SearchMemoryItems([FromQuery] SearchMemoryItemsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
