using backend.Application.Common.Interfaces;
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

    [HttpGet]
    public async Task<ActionResult<List<Member>>> GetAllMembers([FromQuery] string? ids)
    {
        if (!string.IsNullOrEmpty(ids))
        {
            var guids = ids.Split(',').Select(Guid.Parse).ToList();
            var membersByIds = await _memberService.GetMembersByIdsAsync(guids);
            return Ok(membersByIds);
        }
        var members = await _memberService.GetAllMembersAsync();
        return Ok(members);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMemberById(Guid id)
    {
        var member = await _memberService.GetMemberByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return Ok(member);
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember([FromBody] Member member)
    {
        var createdMember = await _memberService.CreateMemberAsync(member);
        return CreatedAtAction(nameof(GetMemberById), new { id = createdMember.Id }, createdMember);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] Member member)
    {
        if (id != member.Id)
        {
            return BadRequest();
        }
        await _memberService.UpdateMemberAsync(member);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        await _memberService.DeleteMemberAsync(id);
        return NoContent();
    }
}