using backend.Application.Common.Models;
using backend.Application.Dashboard.Queries.GetPublicDashboard;
using backend.Application.Events;
using backend.Application.Events.Queries.GetPublicEventById;
using backend.Application.Events.Queries.GetPublicUpcomingEvents;
using backend.Application.Events.Queries.SearchPublicEvents;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Queries.GetPublicFamilyById;
using backend.Application.Families.Queries.SearchPublicFamilies;
using backend.Application.FamilyDicts;
using backend.Application.FamilyDicts.Queries.Public;
using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetPublicMemberById;
using backend.Application.Members.Queries.GetPublicMembersByFamilyId;
using backend.Application.Members.Queries.SearchPublicMembers;
using backend.Application.Relationships.Queries;
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
    public async Task<ActionResult<FamilyDetailDto>> GetPublicFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicFamilyByIdQuery(id));
        return result.IsSuccess ? (ActionResult<FamilyDetailDto>)Ok(result.Value) : (ActionResult<FamilyDetailDto>)NotFound(result.Error);
    }
    [HttpGet("families/search")]
    public async Task<ActionResult<PaginatedList<FamilyListDto>>> SearchPublicFamilies([FromQuery] SearchPublicFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<FamilyListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<FamilyListDto>>)BadRequest(result.Error);
    }
    [HttpGet("family/{familyId}/members")]
    public async Task<ActionResult<List<MemberListDto>>> GetPublicMembersByFamilyId(Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicMembersByFamilyIdQuery(familyId));
        return result.IsSuccess ? (ActionResult<List<MemberListDto>>)Ok(result.Value) : (ActionResult<List<MemberListDto>>)BadRequest(result.Error);
    }
    [HttpGet("family/{familyId}/member/{id}")]
    public async Task<ActionResult<MemberDetailDto>> GetPublicMemberById(Guid id, [FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicMemberByIdQuery(id, familyId));
        return result.IsSuccess ? (ActionResult<MemberDetailDto>)Ok(result.Value) : (ActionResult<MemberDetailDto>)NotFound(result.Error);
    }
    [HttpGet("members/search")]
    public async Task<ActionResult<PaginatedList<MemberListDto>>> SearchPublicMembers([FromQuery] SearchPublicMembersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<MemberListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<MemberListDto>>)BadRequest(result.Error);
    }
    [HttpGet("family/{familyId}/relationships")]
    public async Task<ActionResult<List<RelationshipListDto>>> GetPublicRelationshipsByFamilyId(Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicRelationshipsByFamilyIdQuery(familyId));
        return result.IsSuccess ? (ActionResult<List<RelationshipListDto>>)Ok(result.Value) : (ActionResult<List<RelationshipListDto>>)BadRequest(result.Error);
    }
    [HttpGet("event/{id}")]
    public async Task<ActionResult<EventDto>> GetPublicEventById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicEventByIdQuery(id));
        return result.IsSuccess ? (ActionResult<EventDto>)Ok(result.Value) : (ActionResult<EventDto>)NotFound(result.Error);
    }
    [HttpGet("events/search")]
    public async Task<ActionResult<PaginatedList<EventDto>>> SearchPublicEvents([FromQuery] SearchPublicEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<EventDto>>)Ok(result.Value) : (ActionResult<PaginatedList<EventDto>>)BadRequest(result.Error);
    }
    [HttpGet("events/upcoming")]
    public async Task<ActionResult<List<EventDto>>> GetPublicUpcomingEvents([FromQuery] GetPublicUpcomingEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<List<EventDto>>)Ok(result.Value) : (ActionResult<List<EventDto>>)BadRequest(result.Error);
    }
    [HttpPost("face/detect")]
    public async Task<ActionResult<FaceDetectionResponseDto>> DetectFaces([FromBody] DetectFacesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<FaceDetectionResponseDto>)Ok(result.Value) : (ActionResult<FaceDetectionResponseDto>)BadRequest(result.Error);
    }
    [HttpGet("family-dict")]
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FamilyDictDto>>> GetFamilyDicts([FromQuery] GetPublicFamilyDictsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<FamilyDictDto>>)Ok(result.Value) : (ActionResult<PaginatedList<FamilyDictDto>>)BadRequest(result.Error);
    }
    [HttpGet("family-dict/{id}")]
    [ProducesResponseType(typeof(FamilyDictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDictDto>> GetFamilyDictById(Guid id)
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
    public async Task<ActionResult<PublicDashboardDto>> GetPublicDashboard()
    {
        var result = await _mediator.Send(new GetPublicDashboardQuery());
        return result.IsSuccess ? (ActionResult<PublicDashboardDto>)Ok(result.Value) : (ActionResult<PublicDashboardDto>)BadRequest(result.Error);
    }
}
