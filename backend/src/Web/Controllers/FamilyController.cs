using backend.Application.Common.Interfaces;
using backend.Domain.Entities;
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
    public async Task<ActionResult<List<Family>>> GetAllFamilies([FromQuery] string? ids)
    {
        if (!string.IsNullOrEmpty(ids))
        {
            var guids = ids.Split(',').Select(Guid.Parse).ToList();
            var familiesByIds = await _familyService.GetFamiliesByIdsAsync(guids);
            return Ok(familiesByIds);
        }
        var families = await _familyService.GetAllFamiliesAsync();
        return Ok(families);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Family>> GetFamilyById(Guid id)
    {
        var family = await _familyService.GetFamilyByIdAsync(id);
        if (family == null)
        {
            return NotFound();
        }
        return Ok(family);
    }

    [HttpPost]
    public async Task<ActionResult<Family>> CreateFamily([FromBody] Family family)
    {
        var createdFamily = await _familyService.CreateFamilyAsync(family);
        return CreatedAtAction(nameof(GetFamilyById), new { id = createdFamily.Id }, createdFamily);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFamily(Guid id, [FromBody] Family family)
    {
        if (id != family.Id)
        {
            return BadRequest();
        }
        await _familyService.UpdateFamilyAsync(family);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        await _familyService.DeleteFamilyAsync(id);
        return NoContent();
    }
}