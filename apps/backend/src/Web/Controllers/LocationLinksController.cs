using backend.Application.Common.Constants;
using backend.Application.Common.Security; // Specify ILogger to avoid ambiguity
using backend.Application.LocationLinks.Commands.CreateLocationLink;
using backend.Application.LocationLinks.Commands.DeleteLocationLink;
using backend.Application.LocationLinks.Commands.UpdateLocationLink;
using backend.Application.LocationLinks.Queries; // Add this using statement
using backend.Application.LocationLinks.Queries.GetLocationLinkById;
using backend.Application.LocationLinks.Queries.GetLocationLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/location-links")]
[Authorize]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class LocationLinksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LocationLinksController> _logger;

    public LocationLinksController(IMediator mediator, ILogger<LocationLinksController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLocationLinks([FromQuery] GetLocationLinksQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLocationLinkById(Guid id)
    {
        var result = await _mediator.Send(new GetLocationLinkByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("by-member/{memberId}")]
    public async Task<IActionResult> GetLocationLinksByMemberId(Guid memberId)
    {
        var result = await _mediator.Send(new GetLocationLinksByMemberIdQuery { MemberId = memberId });
        return result.ToActionResult(this, _logger);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLocationLinkCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetLocationLinkById), new { id = result.Value });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateLocationLinkCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteLocationLinkCommand(id));
        return result.ToActionResult(this, _logger, 204);
    }
}
