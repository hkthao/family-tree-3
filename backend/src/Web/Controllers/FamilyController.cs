using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families;
using backend.Domain.Entities;
// using backend.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyController : ControllerBase
{
    private readonly IFamilyService _familyService;

    public FamilyController(IFamilyService familyService)
    {
        _familyService = familyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FamilyDto>>> GetAllFamilies([FromQuery] string? ids)
    {
        Result<List<FamilyDto>> result;
        if (!string.IsNullOrEmpty(ids))
        {
            var guids = ids.Split(',').Select(Guid.Parse).ToList();
            result = await _familyService.GetByIdsAsync(guids);
        }
        else
        {
            result = await _familyService.GetAllAsync();
        }

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FamilyDto>> GetFamilyById(Guid id)
    {
        var result = await _familyService.GetByIdAsync(id);
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound();
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<FamilyDto>>> Search([FromQuery] FamilyFilterModel filter)
    {
        var result = await _familyService.SearchAsync(filter);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpPost]
    public async Task<ActionResult<Family>> CreateFamily([FromBody] Family family)
    {
        var result = await _familyService.CreateAsync(family);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetFamilyById), new { id = result.Value!.Id }, result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFamily(Guid id, [FromBody] Family family)
    {
        if (id != family.Id)
        {
            return BadRequest();
        }
        var result = await _familyService.UpdateAsync(family);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        var result = await _familyService.DeleteAsync(id);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }

    [HttpGet("by-ids")]
    public async Task<ActionResult<FamilyDto>> GetMemberByIds([FromQuery] string ids)
    {
        var _ids = ids.Split(',').Select(e => Guid.Parse(e)).ToList();
        var result = await _familyService.GetByIdsAsync(_ids);
        if (result.IsSuccess)
        {
            if (result.Value == null)
                return NotFound();
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }
}