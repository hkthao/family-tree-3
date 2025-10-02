using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<SearchItem>>> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int itemsPerPage = 10)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest("Keyword cannot be empty.");
        }
        var result = await _searchService.SearchAsync(keyword, page, itemsPerPage);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return StatusCode(500, result.Error); // Or a more specific error code
    }
}