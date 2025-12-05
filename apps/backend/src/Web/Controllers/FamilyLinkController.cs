using backend.Application.FamilyLinks.Commands.ApproveFamilyLinkRequest;
using backend.Application.FamilyLinks.Commands.CreateFamilyLinkRequest;
using backend.Application.FamilyLinks.Commands.RejectFamilyLinkRequest;
using backend.Application.FamilyLinks.Commands.UnlinkFamilies;
using backend.Application.FamilyLinks.Queries;
using backend.Application.FamilyLinks.Queries.GetFamilyLinkById;
using backend.Application.FamilyLinks.Queries.GetFamilyLinkRequests;
using backend.Application.FamilyLinks.Queries.GetFamilyLinks;
using MediatR;
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
    /// Gửi yêu cầu liên kết từ một gia đình đến một gia đình khác.
    /// </summary>
    /// <param name="command">Lệnh chứa ID gia đình gửi yêu cầu và ID gia đình nhận yêu cầu.</param>
    /// <returns>ID của yêu cầu liên kết đã tạo.</returns>
    [HttpPost("request")]
    public async Task<ActionResult<Guid>> CreateFamilyLinkRequest([FromBody] CreateFamilyLinkRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Phê duyệt một yêu cầu liên kết gia đình.
    /// </summary>
    /// <param name="requestId">ID của yêu cầu liên kết cần phê duyệt.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPost("request/{requestId}/approve")]
    public async Task<ActionResult> ApproveFamilyLinkRequest(Guid requestId)
    {
        var result = await _mediator.Send(new ApproveFamilyLinkRequestCommand(requestId));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Từ chối một yêu cầu liên kết gia đình.
    /// </summary>
    /// <param name="requestId">ID của yêu cầu liên kết cần từ chối.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPost("request/{requestId}/reject")]
    public async Task<ActionResult> RejectFamilyLinkRequest(Guid requestId)
    {
        var result = await _mediator.Send(new RejectFamilyLinkRequestCommand(requestId));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Hủy liên kết giữa hai gia đình.
    /// </summary>
    /// <param name="family1Id">ID của gia đình thứ nhất.</param>
    /// <param name="family2Id">ID của gia đình thứ hai.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("unlink/{family1Id}/{family2Id}")]
    public async Task<ActionResult> UnlinkFamilies(Guid family1Id, Guid family2Id)
    {
        var result = await _mediator.Send(new UnlinkFamiliesCommand(family1Id, family2Id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy tất cả các yêu cầu liên kết liên quan đến một gia đình (gửi đi và nhận về).
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách các yêu cầu liên kết gia đình.</returns>
    [HttpGet("requests/{familyId}")]
    public async Task<ActionResult<List<FamilyLinkRequestDto>>> GetFamilyLinkRequests(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyLinkRequestsQuery(familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy tất cả các liên kết gia đình đang hoạt động cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách các liên kết gia đình.</returns>
    [HttpGet("links/{familyId}")]
    public async Task<ActionResult<List<FamilyLinkDto>>> GetFamilyLinks(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyLinksQuery(familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
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
