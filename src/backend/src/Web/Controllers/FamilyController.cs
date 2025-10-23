using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Application.Families.Commands.CreateFamilies;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Queries.SearchFamilies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FamilyController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<FamilyDto>>> GetAllFamilies([FromQuery] string ids)
    {
        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetFamiliesByIdsQuery(guids));
        return result.IsSuccess ? (ActionResult<List<FamilyDto>>)Ok(result.Value) : (ActionResult<List<FamilyDto>>)BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FamilyDto>> GetFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error); // Assuming NotFound for single item retrieval failure
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<FamilyDto>>> Search([FromQuery] SearchFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<FamilyDto>>)Ok(result.Value) : (ActionResult<PaginatedList<FamilyDto>>)BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateFamily([FromBody] CreateFamilyCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Guid>)CreatedAtAction(nameof(GetFamilyById), new { id = result.Value }, result.Value) : (ActionResult<Guid>)BadRequest(result.Error);
    }

    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateFamilies([FromBody] CreateFamiliesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<Guid>>)Ok(result.Value) : (ActionResult<List<Guid>>)BadRequest(result.Error);
    }

    [HttpPost("generate-family-data")]
    public async Task<ActionResult<List<FamilyDto>>> GenerateFamilyData([FromBody] GenerateFamilyDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<FamilyDto>>)Ok(result.Value) : (ActionResult<List<FamilyDto>>)BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFamily(Guid id, [FromBody] UpdateFamilyCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet("by-ids")]
    public async Task<ActionResult<List<FamilyDto>>> GetFamiliesByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return Ok(Result<List<FamilyDto>>.Success([]).Value);

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetFamiliesByIdsQuery(guids));
        return result.IsSuccess ? (ActionResult<List<FamilyDto>>)Ok(result.Value) : (ActionResult<List<FamilyDto>>)BadRequest(result.Error);
    }
}
