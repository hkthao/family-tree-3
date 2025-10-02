using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
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
        Result<List<Member>> result;
        if (!string.IsNullOrEmpty(ids))
        {
            var guids = ids.Split(',').Select(Guid.Parse).ToList();
            result = await _memberService.GetMembersByIdsAsync(guids);
        }
        else
        {
            result = await _memberService.GetAllAsync(); // Use GetAllAsync from IBaseCrudService
        }

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error); // Or a more specific error code
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Member>> GetMemberById(Guid id)
    {
        var result = await _memberService.GetByIdAsync(id); // Use GetByIdAsync from IBaseCrudService
        if (result.IsSuccess)
        {
            if (result.Value == null)
            {
                return NotFound();
            }
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error); // Or a more specific error code
    }

    [HttpPost]
    public async Task<ActionResult<Member>> CreateMember([FromBody] Member member)
    {
        var result = await _memberService.CreateAsync(member); // Use CreateAsync from IBaseCrudService
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetMemberById), new { id = result.Value!.Id }, result.Value);
        }
        return StatusCode(500, result.Error); // Or a more specific error code
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] Member member)
    {
        if (id != member.Id)
        {
            return BadRequest();
        }
        var result = await _memberService.UpdateAsync(member); // Use UpdateAsync from IBaseCrudService
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error); // Or a more specific error code
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var result = await _memberService.DeleteAsync(id); // Use DeleteAsync from IBaseCrudService
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error); // Or a more specific error code
    }
}