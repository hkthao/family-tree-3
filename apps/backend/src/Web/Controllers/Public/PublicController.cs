using backend.Application.Common.Models;
using backend.Application.Dashboard.Queries.GetPublicDashboard;
using backend.Application.Events.Queries.GetPublicEventById;
using backend.Application.Events.Queries.GetPublicUpcomingEvents;
using backend.Application.Events.Queries.SearchPublicEvents;
using backend.Application.Families.Queries.GetPublicFamilyById;
using backend.Application.Families.Queries.SearchPublicFamilies;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Queries.Public;
using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.Members.Queries.GetPublicMemberById;
using backend.Application.Members.Queries.GetPublicMembersByFamilyId;
using backend.Application.Members.Queries.SearchPublicMembers;
using backend.Application.Relationships.Queries.GetPublicRelationshipsByFamilyId;
using backend.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/public")]
[TypeFilter(typeof(ApiKeyAuthenticationFilter))]
[TypeFilter(typeof(BotDetectionActionFilter))]
public class PublicController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("family/{id}")]
    public async Task<IActionResult> GetPublicFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicFamilyByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    [HttpGet("families/search")]
    public async Task<IActionResult> SearchPublicFamilies([FromQuery] SearchPublicFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("family/{familyId}/members")]
    public async Task<IActionResult> GetPublicMembersByFamilyId(Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicMembersByFamilyIdQuery(familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("family/{familyId}/member/{id}")]
    public async Task<IActionResult> GetPublicMemberById(Guid id, [FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicMemberByIdQuery(id, familyId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    [HttpGet("members/search")]
    public async Task<IActionResult> SearchPublicMembers([FromQuery] SearchPublicMembersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("family/{familyId}/relationships")]
    public async Task<IActionResult> GetPublicRelationshipsByFamilyId(Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicRelationshipsByFamilyIdQuery(familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("event/{id}")]
    public async Task<IActionResult> GetPublicEventById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicEventByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
    [HttpGet("events/search")]
    public async Task<IActionResult> SearchPublicEvents([FromQuery] SearchPublicEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("events/upcoming")]
    public async Task<IActionResult> GetPublicUpcomingEvents([FromQuery] GetPublicUpcomingEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpPost("face/detect")]
    public async Task<IActionResult> DetectFaces([FromBody] DetectFacesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("family-dict")]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFamilyDicts([FromQuery] GetPublicFamilyDictsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpGet("family-dict/{id}")]
    [ProducesResponseType(typeof(FamilyDictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyDictById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicFamilyDictByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(PublicDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicDashboard([FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicDashboardQuery(familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}