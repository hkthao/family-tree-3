using backend.Application.Common.Models;
using backend.Application.FamilyMedia.Commands.CreateFamilyMedia;
using backend.Application.FamilyMedia.Commands.DeleteFamilyMedia;
using backend.Application.FamilyMedia.Commands.LinkMediaToEntity;
using backend.Application.FamilyMedia.Commands.UnlinkMediaFromEntity;
using backend.Application.FamilyMedia.DTOs;
using backend.Application.FamilyMedia.Queries.GetFamilyMediaById;
using backend.Application.FamilyMedia.Queries.GetFamilyMediaList;
using backend.Application.FamilyMedia.Queries.GetMediaLinksByFamilyMediaId;
using backend.Application.FamilyMedia.Queries.GetMediaLinksByRefId;
using backend.Domain.Enums; // For RefType, MediaType
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family/{familyId}/media")]
public class FamilyMediaController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Tải lên một file media mới cho một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="command">Lệnh chứa thông tin file media và IFormFile.</param>
    /// <returns>ID của file media vừa được tạo.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")] // Specify content type for file uploads
    public async Task<IActionResult> CreateFamilyMedia(Guid familyId, [FromForm] CreateFamilyMediaCommand command)
    {
        if (familyId != command.FamilyId)
        {
            return BadRequest("FamilyId in route does not match command body.");
        }

        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetFamilyMediaById), new { familyId, id = result.Value }, result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một file media theo ID.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="id">ID của file media cần lấy.</param>
    /// <returns>Thông tin chi tiết của file media.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFamilyMediaById(Guid familyId, Guid id)
    {
        var result = await _mediator.Send(new GetFamilyMediaByIdQuery(id, familyId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Lấy danh sách các file media cho một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="filters">Bộ lọc file media.</param>
    /// <param name="pageNumber">Số trang.</param>
    /// <param name="pageSize">Kích thước trang.</param>
    /// <param name="orderBy">Sắp xếp theo trường.</param>
    /// <returns>Danh sách phân trang các file media.</returns>
    [HttpGet]
    public async Task<IActionResult> GetFamilyMediaList(
        Guid familyId,
        [FromQuery] FamilyMediaFilter filters,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? orderBy = null)
    {
        var result = await _mediator.Send(new GetFamilyMediaListQuery(familyId, filters, pageNumber, pageSize, orderBy));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Xóa một file media.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="id">ID của file media cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamilyMedia(Guid familyId, Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyMediaCommand { Id = id, FamilyId = familyId });
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Liên kết một file media với một thực thể (thành viên, câu chuyện, v.v.).
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="familyMediaId">ID của file media.</param>
    /// <param name="command">Lệnh chứa thông tin liên kết.</param>
    /// <returns>ID của MediaLink vừa được tạo.</returns>
    [HttpPost("{familyMediaId}/link")]
    public async Task<IActionResult> LinkMediaToEntity(Guid familyId, Guid familyMediaId, [FromBody] LinkMediaToEntityCommand command)
    {
        if (familyMediaId != command.FamilyMediaId)
        {
            return BadRequest("FamilyMediaId in route does not match command body.");
        }
        // Assuming familyId is implicitly handled by authorization within the command handler
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Created("", result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Hủy liên kết một file media khỏi một thực thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="familyMediaId">ID của file media.</param>
    /// <param name="refType">Loại thực thể được liên kết (Member, MemberStory, v.v.).</param>
    /// <param name="refId">ID của thực thể được liên kết.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{familyMediaId}/link/{refType}/{refId}")]
    public async Task<IActionResult> UnlinkMediaFromEntity(Guid familyId, Guid familyMediaId, RefType refType, Guid refId)
    {
        // Assuming familyId is implicitly handled by authorization within the command handler
        var result = await _mediator.Send(new UnlinkMediaFromEntityCommand { FamilyMediaId = familyMediaId, RefType = refType, RefId = refId });
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy tất cả các liên kết media cho một file media cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="familyMediaId">ID của file media.</param>
    /// <returns>Danh sách các MediaLinkDto.</returns>
    [HttpGet("{familyMediaId}/links")]
    public async Task<IActionResult> GetMediaLinksByFamilyMediaId(Guid familyId, Guid familyMediaId)
    {
        var result = await _mediator.Send(new GetMediaLinksByFamilyMediaIdQuery(familyMediaId, familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy tất cả các liên kết media cho một thực thể cụ thể (thành viên, câu chuyện, v.v.).
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="refType">Loại thực thể được liên kết (Member, MemberStory, v.v.).</param>
    /// <param name="refId">ID của thực thể được liên kết.</param>
    /// <returns>Danh sách các MediaLinkDto.</returns>
    [HttpGet("links/{refType}/{refId}")]
    public async Task<IActionResult> GetMediaLinksByRefId(Guid familyId, RefType refType, Guid refId)
    {
        var result = await _mediator.Send(new GetMediaLinksByRefIdQuery(refId, refType, familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
