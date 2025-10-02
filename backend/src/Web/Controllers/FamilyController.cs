using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FamilyController : ControllerBase
{
    private readonly IFamilyService _familyService;
    private readonly ISearchService _searchService;

    public FamilyController(IFamilyService familyService, ISearchService searchService)
    {
        _familyService = familyService;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Family>>> GetAllFamilies([FromQuery] string? ids)
    {
        Result<List<Family>> result;
        if (!string.IsNullOrEmpty(ids))
        {
            var guids = ids.Split(',').Select(Guid.Parse).ToList();
            result = await _familyService.GetFamiliesByIdsAsync(guids);
        }
        else
        {
            result = await _familyService.GetAllAsync(); // Use GetAllAsync from IBaseCrudService
        }

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Family>> GetFamilyById(Guid id)
    {
        var result = await _familyService.GetByIdAsync(id); // Use GetByIdAsync from IBaseCrudService
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
    public async Task<ActionResult<Family>> CreateFamily([FromBody] Family family)
    {
        var result = await _familyService.CreateAsync(family); // Use CreateAsync from IBaseCrudService
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
        var result = await _familyService.UpdateAsync(family); // Use UpdateAsync from IBaseCrudService
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        var result = await _familyService.DeleteAsync(id); // Use DeleteAsync from IBaseCrudService
        if (result.IsSuccess)
        {
            return NoContent();
        }
        return StatusCode(500, result.Error);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<Family>>> SearchFamilies(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int itemsPerPage = 10)
    {
        var result = await _familyService.SearchFamiliesAsync(keyword, page, itemsPerPage);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error);
    }
}