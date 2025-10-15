using backend.Application.Common.Models;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.Members.Commands.GenerateMemberData;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMembersByIds;
using backend.Application.Members.Queries.SearchMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
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
    public async Task<ActionResult<PaginatedList<MemberListDto>>> Search([FromQuery] SearchMembersQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error); // Or other appropriate error handling
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDetailDto>> GetMemberById(Guid id)
    {
        var result = await _mediator.Send(new GetMemberByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error); // Assuming NotFound for single item retrieval failure
    }

    [HttpGet("by-ids")]
    public async Task<ActionResult<List<MemberListDto>>> GetMembersByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return Ok(Result<List<MemberListDto>>.Success(new List<MemberListDto>()).Value);
        }

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetMembersByIdsQuery(guids));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error); // Or other appropriate error handling
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateMember([FromBody] CreateMemberCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMemberById), new { id = result.Value }, result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPost("generate-member-data")]
    public async Task<ActionResult<List<MemberDto>>> GenerateMemberData([FromBody] GenerateMemberDataCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPost("generate-member-data")]
    public async Task<ActionResult<List<MemberDto>>> GenerateMemberData([FromBody] GenerateMemberDataCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateMembers([FromBody] CreateMembersCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberCommand command)
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
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var result = await _mediator.Send(new DeleteMemberCommand(id));
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return BadRequest(result.Error);
    }
}
