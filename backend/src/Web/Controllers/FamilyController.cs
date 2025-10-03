using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Queries.SearchFamilies;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyController : ControllerBase
{
    private readonly IMediator _mediator;

    public FamilyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<FamilyDto>>> GetAllFamilies([FromQuery] string ids)
    {
        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        return await _mediator.Send(new GetFamiliesByIdsQuery(guids));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FamilyDto>> GetFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyByIdQuery(id));
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<FamilyDto>>> Search([FromQuery] FamilyFilterModel filter)
    {
        return await _mediator.Send(new SearchFamiliesQuery { Keyword = filter.SearchQuery, PageNumber = filter.Page, PageSize = filter.ItemsPerPage });
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateFamily([FromBody] CreateFamilyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetFamilyById), new { id = result }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFamily(Guid id, [FromBody] UpdateFamilyCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        await _mediator.Send(new DeleteFamilyCommand(id));
        return NoContent();
    }

    [HttpGet("by-ids")]
    public async Task<ActionResult<List<FamilyDto>>> GetFamiliesByIds([FromQuery] string ids)
    {
        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        return await _mediator.Send(new GetFamiliesByIdsQuery(guids));
    }
}