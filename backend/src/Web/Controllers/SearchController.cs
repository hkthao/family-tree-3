using backend.Application.Common.Models;
using backend.Application.Search.Queries.Search;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
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
        return await _mediator.Send(new SearchQuery { Keyword = keyword, PageNumber = page, PageSize = itemsPerPage });
    }
}