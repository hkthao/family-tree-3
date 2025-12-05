using backend.Application.Common.Models; // New import for PaginatedList
using backend.Application.FamilyLinks.Commands.DeleteLinkFamily;
using backend.Application.FamilyLinks.Queries;
using backend.Application.FamilyLinks.Queries.GetFamilyLinkById;
using backend.Application.FamilyLinks.Queries.SearchFamilyLinks; // New import
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-link")]
public class FamilyLinkController(IMediator mediator, ILogger<FamilyLinkController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FamilyLinkController> _logger = logger;

    /// <summary>
    /// Hủy liên kết giữa hai gia đình.
    /// </summary>
    /// <param name="family1Id">ID của gia đình thứ nhất.</param>
    /// <param name="family2Id">ID của gia đình thứ hai.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("delete/{familyLinkId}")] // Changed route
    public async Task<ActionResult> DeleteFamilyLink(Guid familyLinkId) // Changed signature
    {
        var result = await _mediator.Send(new DeleteLinkFamilyCommand(familyLinkId)); // Updated command
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Tìm kiếm và phân trang các liên kết gia đình đang hoạt động cho một gia đình cụ thể.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các liên kết gia đình tìm được.</returns>
    [HttpGet("search")] // Renamed from 'links/{familyId}' to 'search'
    public async Task<ActionResult<PaginatedList<FamilyLinkDto>>> Search([FromQuery] SearchFamilyLinksQuery query) // Renamed and changed signature
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<FamilyLinkDto>>)Ok(result.Value) : (ActionResult<PaginatedList<FamilyLinkDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một liên kết gia đình cụ thể.
    /// </summary>
    /// <param name="familyLinkId">ID của liên kết gia đình.</param>
    /// <returns>Thông tin chi tiết của liên kết gia đình.</returns>
    [HttpGet("links/by-id/{familyLinkId}")]
    public async Task<ActionResult<FamilyLinkDto>> GetFamilyLinkById(Guid familyLinkId)
    {
        var result = await _mediator.Send(new GetFamilyLinkByIdQuery(familyLinkId));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
