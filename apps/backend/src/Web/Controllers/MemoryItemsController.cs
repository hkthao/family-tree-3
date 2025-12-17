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
public class MemoryItemsController : ApiControllerBase
{
    [HttpPost("family/{familyId}/memory-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<Guid>>> CreateMemoryItem(Guid familyId, CreateMemoryItemCommand command)
    {
        if (familyId != command.FamilyId)
        {
            return BadRequest(Result<Guid>.Failure("Family ID in route must match Family ID in body."));
        }
        var result = await Mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpPut("family/{familyId}/memory-items/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> UpdateMemoryItem(Guid familyId, Guid id, UpdateMemoryItemCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(Result.Failure("Memory Item ID in route must match Memory Item ID in body."));
        }
        if (familyId != command.FamilyId)
        {
            return BadRequest(Result.Failure("Family ID in route must match Family ID in body."));
        }
        var result = await Mediator.Send(command);
        return result.ToActionResult();
    }

    [HttpDelete("family/{familyId}/memory-items/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> DeleteMemoryItem(Guid familyId, Guid id)
    {
        var result = await Mediator.Send(new DeleteMemoryItemCommand { Id = id, FamilyId = familyId });
        return result.ToActionResult();
    }

    [HttpGet("family/{familyId}/memory-items/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<MemoryItemDto>>> GetMemoryItemDetail(Guid familyId, Guid id)
    {
        var result = await Mediator.Send(new GetMemoryItemDetailQuery { Id = id, FamilyId = familyId });
        return result.ToActionResult();
    }

    [HttpGet("family/{familyId}/memory-items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<MemoryItemDto>>> SearchMemoryItems(Guid familyId, [FromQuery] SearchMemoryItemsQuery query)
    {
        if (familyId != query.FamilyId)
        {
            return BadRequest(Result<PaginatedList<MemoryItemDto>>.Failure("Family ID in route must match Family ID in query."));
        }
        return await Mediator.Send(query);
    }
}
