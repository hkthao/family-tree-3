using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Application.Relationships.Queries;
using backend.Application.Relationships.Queries.GetRelationshipById;
using backend.Application.Relationships.Queries.GetRelationships;
using backend.Application.Relationships.Queries.SearchRelationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RelationshipsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RelationshipsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<RelationshipListDto>>> GetRelationships([FromQuery] GetRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RelationshipDto>> GetRelationshipById(Guid id)
    {
        var result = await _mediator.Send(new GetRelationshipByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<RelationshipListDto>>> Search([FromQuery] SearchRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRelationship([FromBody] CreateRelationshipCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetRelationshipById), new { id = result.Value }, result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelationship(Guid id, [FromBody] UpdateRelationshipCommand command)
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
    public async Task<IActionResult> DeleteRelationship(Guid id)
    {
        var result = await _mediator.Send(new DeleteRelationshipCommand(id));
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return BadRequest(result.Error);
    }
}
