using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.Relationships.Commands.GenerateRelationshipData;
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
public class RelationshipsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PaginatedList<RelationshipListDto>>> GetRelationships([FromQuery] GetRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<RelationshipListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<RelationshipListDto>>)BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RelationshipDto>> GetRelationshipById(Guid id)
    {
        var result = await _mediator.Send(new GetRelationshipByIdQuery(id));
        return result.IsSuccess ? (ActionResult<RelationshipDto>)Ok(result.Value) : (ActionResult<RelationshipDto>)NotFound(result.Error);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<RelationshipListDto>>> Search([FromQuery] SearchRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<RelationshipListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<RelationshipListDto>>)BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRelationship([FromBody] CreateRelationshipCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? (ActionResult<Guid>)CreatedAtAction(nameof(GetRelationshipById), new { id = result.Value }, result.Value)
            : (ActionResult<Guid>)BadRequest(result.Error);
    }

    [HttpPost("generate-relationship-data")]
    public async Task<ActionResult<List<RelationshipDto>>> GenerateRelationshipData([FromBody] GenerateRelationshipDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<RelationshipDto>>)Ok(result.Value) : (ActionResult<List<RelationshipDto>>)BadRequest(result.Error);
    }

    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateRelationships([FromBody] CreateRelationshipsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<Guid>>)Ok(result.Value) : (ActionResult<List<Guid>>)BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelationship(Guid id, [FromBody] UpdateRelationshipCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelationship(Guid id)
    {
        var result = await _mediator.Send(new DeleteRelationshipCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
