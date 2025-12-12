using backend.Application.Common.Models;
using backend.Application.FamilyLinkRequests.Commands.ApproveFamilyLinkRequest;
using backend.Application.FamilyLinkRequests.Commands.CreateFamilyLinkRequest;
using backend.Application.FamilyLinkRequests.Commands.DeleteFamilyLinkRequest; // New import
using backend.Application.FamilyLinkRequests.Commands.RejectFamilyLinkRequest;
using backend.Application.FamilyLinkRequests.Queries.GetFamilyLinkRequestById; // New import
using backend.Application.FamilyLinkRequests.Queries.SearchFamilyLinkRequests;
using backend.Application.FamilyLinks.Queries; // Keep this for FamilyLinkRequestDto
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Web.Infrastructure; // Added

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-link-requests")]
public class FamilyLinkRequestsController(IMediator mediator, ILogger<FamilyLinkRequestsController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FamilyLinkRequestsController> _logger = logger;

    /// <summary>
    /// Gửi yêu cầu liên kết từ một gia đình đến một gia đình khác.
    /// </summary>
    /// <param name="command">Lệnh chứa ID gia đình gửi yêu cầu và ID gia đình nhận yêu cầu.</param>
    /// <returns>ID của yêu cầu liên kết đã tạo.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFamilyLinkRequest([FromBody] CreateFamilyLinkRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Lấy một yêu cầu liên kết gia đình theo ID.
    /// </summary>
    /// <param name="id">ID của yêu cầu liên kết.</param>
    /// <returns>Yêu cầu liên kết gia đình.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FamilyLinkRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyLinkRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyLinkRequestByIdQuery(id));
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Xóa một yêu cầu liên kết gia đình hiện có.
    /// </summary>
    /// <param name="id">ID của yêu cầu liên kết cần xóa.</param>
    /// <returns>NoContent nếu xóa thành công.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFamilyLinkRequest(Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyLinkRequestCommand(id));
        return result.ToActionResult(this, 204);
    }

    /// <summary>
    /// Phê duyệt một yêu cầu liên kết gia đình.
    /// </summary>
    /// <param name="requestId">ID của yêu cầu liên kết cần phê duyệt.</param>
    /// <param name="responseMessage">Tin nhắn phản hồi khi phê duyệt.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPost("{requestId}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveFamilyLinkRequest(Guid requestId, [FromBody] string? responseMessage = null)
    {
        var result = await _mediator.Send(new ApproveFamilyLinkRequestCommand(requestId, responseMessage));
        return result.ToActionResult(this, 204);
    }

    /// <summary>
    /// Từ chối một yêu cầu liên kết gia đình.
    /// </summary>
    /// <param name="requestId">ID của yêu cầu liên kết cần từ chối.</param>
    /// <param name="responseMessage">Tin nhắn phản hồi khi từ chối.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPost("{requestId}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectFamilyLinkRequest(Guid requestId, [FromBody] string? responseMessage = null)
    {
        var result = await _mediator.Send(new RejectFamilyLinkRequestCommand(requestId, responseMessage));
        return result.ToActionResult(this, 204);
    }

    /// <summary>
    /// Lấy tất cả các yêu cầu liên kết liên quan đến một gia đình (gửi đi và nhận về), có hỗ trợ phân trang, lọc và sắp xếp.
    /// </summary>
    /// <returns>Danh sách phân trang các yêu cầu liên kết gia đình.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedList<FamilyLinkRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFamilyLinkRequests([FromQuery] SearchFamilyLinkRequestsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this);
    }
}
