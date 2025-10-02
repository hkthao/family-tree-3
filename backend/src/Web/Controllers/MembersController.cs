using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Members;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<MemberDto>>> Search([FromQuery] MemberFilterModel filter)
    {
        var result = await _memberService.SearchAsync(filter);
        if (result.IsSuccess)
            return Ok(result.Value);
        return StatusCode(500, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetMemberById(Guid id)
    {
        var result = await _memberService.GetByIdAsync(id);
        if (result.IsSuccess)
        {
            if (result.Value == null)
            {
                return NotFound();
            }
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> CreateMember([FromBody] Member member)
    {
        var result = await _memberService.CreateAsync(member);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMemberById), new { id = result.Value!.Id }, result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] Member member)
    {
        if (id != member.Id)
        {
            return BadRequest();
        }
        var result = await _memberService.UpdateAsync(member);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var result = await _memberService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }
}