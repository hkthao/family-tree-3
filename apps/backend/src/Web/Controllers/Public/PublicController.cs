using backend.Application.Common.Models;
using backend.Application.Events;
using backend.Application.Events.Queries.GetPublicEventById;
using backend.Application.Events.Queries.GetPublicUpcomingEvents;
using backend.Application.Events.Queries.SearchPublicEvents;
using backend.Application.Faces.Commands.DetectFaces; // Added
using backend.Application.Families.Queries.GetFamilies;
using backend.Application.Families.Queries.GetFamilyById; // Added missing using directive
using backend.Application.Families.Queries.GetPublicFamilyById;
using backend.Application.Families.Queries.SearchPublicFamilies;
using backend.Application.FamilyDicts; // Thêm dòng này từ PublicFamilyDictsController
using backend.Application.FamilyDicts.Queries.Public; // Thêm dòng này từ PublicFamilyDictsController
using backend.Application.Members.Queries.GetMemberById; // Added missing using directive
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetPublicMemberById;
using backend.Application.Members.Queries.GetPublicMembersByFamilyId;
using backend.Application.Members.Queries.SearchPublicMembers; // Add this using directive
using backend.Application.Relationships.Queries; // Add this using directive
using backend.Application.Relationships.Queries.GetPublicRelationshipsByFamilyId; // Add this using directive
using backend.Web.Filters; // Thêm dòng này
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu công khai liên quan đến gia đình và thành viên, không yêu cầu xác thực.
/// </summary>
[ApiController]
[Route("api/public")]
[TypeFilter(typeof(ApiKeyAuthenticationFilter))] // Áp dụng bộ lọc xác thực API Key
[TypeFilter(typeof(BotDetectionActionFilter))] // Áp dụng bộ lọc phát hiện bot cho tất cả các hành động trong controller này
public class PublicController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Lấy thông tin chi tiết của một gia đình công khai theo ID.
    /// </summary>
    /// <param name="id">ID của gia đình cần lấy.</param>
    /// <returns>Thông tin chi tiết của gia đình công khai.</returns>
    [HttpGet("family/{id}")]
    public async Task<ActionResult<FamilyDetailDto>> GetPublicFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicFamilyByIdQuery(id));
        return result.IsSuccess ? (ActionResult<FamilyDetailDto>)Ok(result.Value) : (ActionResult<FamilyDetailDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Tìm kiếm các gia đình công khai dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các gia đình công khai tìm được.</param>
    [HttpGet("families/search")]
    public async Task<ActionResult<PaginatedList<FamilyListDto>>> SearchPublicFamilies([FromQuery] SearchPublicFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<FamilyListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<FamilyListDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy danh sách các thành viên của một gia đình công khai theo Family ID.
    /// </summary>
    /// <param name="familyId">ID của gia đình công khai cần lấy thành viên.</param>
    /// <returns>Danh sách các thành viên thuộc gia đình công khai.</returns>
    [HttpGet("family/{familyId}/members")]
    public async Task<ActionResult<List<MemberListDto>>> GetPublicMembersByFamilyId(Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicMembersByFamilyIdQuery(familyId));
        return result.IsSuccess ? (ActionResult<List<MemberListDto>>)Ok(result.Value) : (ActionResult<List<MemberListDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một thành viên cụ thể trong một gia đình công khai.
    /// </summary>
    /// <param name="id">ID của thành viên.</param>
    /// <param name="familyId">ID của gia đình mà thành viên thuộc về.</param>
    /// <returns>Thông tin chi tiết của thành viên.</returns>
    [HttpGet("family/{familyId}/member/{id}")]
    public async Task<ActionResult<MemberDetailDto>> GetPublicMemberById(Guid id, [FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new GetPublicMemberByIdQuery(id, familyId));
        return result.IsSuccess ? (ActionResult<MemberDetailDto>)Ok(result.Value) : (ActionResult<MemberDetailDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Lấy danh sách các mối quan hệ của một gia đình công khai theo Family ID.
    /// </summary>
    /// <param name="familyId">ID của gia đình công khai cần lấy mối quan hệ.</param>
    /// <returns>Một PaginatedList chứa danh sách các thành viên công khai tìm được.</param>
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

    /// <summary>
    /// Lấy thông tin chi tiết của một sự kiện công khai theo ID.
    /// </summary>
    /// <param name="id">ID của sự kiện cần lấy.</param>
    /// <returns>Thông tin chi tiết của sự kiện công khai.</returns>
    [HttpGet("event/{id}")]
    public async Task<ActionResult<EventDto>> GetPublicEventById(Guid id)
    {
        var result = await _mediator.Send(new GetPublicEventByIdQuery(id));
        return result.IsSuccess ? (ActionResult<EventDto>)Ok(result.Value) : (ActionResult<EventDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Tìm kiếm các sự kiện công khai dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các sự kiện công khai tìm được.</param>
    [HttpGet("events/search")]
    public async Task<ActionResult<PaginatedList<EventDto>>> SearchPublicEvents([FromQuery] SearchPublicEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<EventDto>>)Ok(result.Value) : (ActionResult<PaginatedList<EventDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy danh sách các sự kiện công khai sắp diễn ra.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí lọc (ví dụ: FamilyId, StartDate, EndDate).</param>
    /// <returns>Danh sách các sự kiện công khai sắp diễn ra.</returns>
    [HttpGet("events/upcoming")]
    public async Task<ActionResult<List<EventDto>>> GetPublicUpcomingEvents([FromQuery] GetPublicUpcomingEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<List<EventDto>>)Ok(result.Value) : (ActionResult<List<EventDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Phát hiện khuôn mặt trong một hình ảnh được cung cấp.
    /// </summary>
    /// <param name="command">Đối tượng chứa dữ liệu hình ảnh và các tùy chọn phát hiện.</param>
    /// <returns>Danh sách các khuôn mặt được phát hiện cùng với thông tin liên quan.</returns>
    [HttpPost("face/detect")]
    public async Task<ActionResult<FaceDetectionResponseDto>> DetectFaces([FromBody] DetectFacesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<FaceDetectionResponseDto>)Ok(result.Value) : (ActionResult<FaceDetectionResponseDto>)BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy tất cả các FamilyDict công khai.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa thông tin phân trang.</param>
    /// <returns>Danh sách các FamilyDict công khai được phân trang.</returns>
    [HttpGet("family-dict")] // Adjusted route
    [ProducesResponseType(typeof(PaginatedList<FamilyDictDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<FamilyDictDto>>> GetFamilyDicts([FromQuery] GetPublicFamilyDictsQuery query)
    {
        return await _mediator.Send(query);
    }

    /// <summary>
    /// Lấy chi tiết một FamilyDict công khai theo ID.
    /// </summary>
    /// <param name="id">ID của FamilyDict.</param>
    /// <returns>Chi tiết FamilyDict hoặc NotFound nếu không tìm thấy.</returns>
    [HttpGet("family-dict/{id}")] // Adjusted route
    [ProducesResponseType(typeof(FamilyDictDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDictDto>> GetFamilyDictById(Guid id)
    {
        var familyDict = await _mediator.Send(new GetPublicFamilyDictByIdQuery(id));
        return familyDict == null ? NotFound() : Ok(familyDict);
    }
}
