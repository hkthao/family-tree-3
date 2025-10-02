using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembersByIds;
using backend.Application.Members.Queries.SearchMembers;
using backend.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<MemberDto>>> Search([FromQuery] MemberFilterModel filter)
    {
        return await _mediator.Send(new SearchMembersQuery { Keyword = filter.Keyword, PageNumber = filter.PageNumber, PageSize = filter.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetMemberById(Guid id)
    {
        var result = await _mediator.Send(new GetMemberByIdQuery(id));
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet("by-ids")]
    public async Task<ActionResult<List<MemberDto>>> GetMembersByIds([FromQuery] string ids)
    {
        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        return await _mediator.Send(new GetMembersByIdsQuery(guids));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateMember([FromBody] CreateMemberCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetMemberById), new { id = result }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        await _mediator.Send(new DeleteMemberCommand(id));
        return NoContent();
    }
}