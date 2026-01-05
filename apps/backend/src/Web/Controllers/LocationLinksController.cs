using backend.Application.Common.Constants;
using backend.Application.Common.Security; // Specify ILogger to avoid ambiguity
using backend.Application.LocationLinks.Commands.CreateLocationLink;
using backend.Application.LocationLinks.Commands.DeleteLocationLink;
using backend.Application.LocationLinks.Commands.UpdateLocationLink;
using backend.Application.LocationLinks.Queries.GetLocationLinkById;
using backend.Application.LocationLinks.Queries.GetLocationLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FamilyTree.Web.Controllers;


[ApiController]
[Route("api/location-links")]
[Authorize]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class LocationLinksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LocationLinksController> _logger; // Inject ILogger

    public LocationLinksController(IMediator mediator, ILogger<LocationLinksController> logger) // Update constructor
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLocationLinks([FromQuery] GetLocationLinksQuery query) // Change return type to IActionResult
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger); // Apply ToActionResult
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLocationLinkById(Guid id) // Change return type to IActionResult
    {
        var result = await _mediator.Send(new GetLocationLinkByIdQuery(id));
        return result.ToActionResult(this, _logger); // Apply ToActionResult
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLocationLinkCommand command) // Change return type to IActionResult
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetLocationLinkById), new { id = result.Value }); // Apply ToActionResult with CreatedAtAction
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateLocationLinkCommand command) // Change return type to IActionResult
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204); // Apply ToActionResult with 204 No Content
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) // Change return type to IActionResult
    {
        var result = await _mediator.Send(new DeleteLocationLinkCommand(id));
        return result.ToActionResult(this, _logger, 204); // Apply ToActionResult with 204 No Content
    }
}
