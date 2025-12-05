using backend.Application.FamilyLinks.Commands.ApproveFamilyLinkRequest;
using backend.Application.FamilyLinks.Commands.CreateFamilyLinkRequest;
using backend.Application.FamilyLinks.Commands.RejectFamilyLinkRequest;
using backend.Application.FamilyLinks.Queries;
using backend.Application.FamilyLinks.Queries.GetFamilyLinkRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-link-requests")] // New route for requests
public class FamilyLinkRequestsController(IMediator mediator, ILogger<FamilyLinkRequestsController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FamilyLinkRequestsController> _logger = logger;

    /// <summary>
    /// Gửi yêu cầu liên kết từ một gia đình đến một gia đình khác.
    /// </summary>
    /// <param name="command">Lệnh chứa ID gia đình gửi yêu cầu và ID gia đình nhận yêu cầu.</param>
    /// <returns>ID của yêu cầu liên kết đã tạo.</returns>
    [HttpPost] // Route will be api/family-link-requests
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
    [HttpPost("{requestId}/approve")]
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
    [HttpPost("{requestId}/reject")]
    public async Task<ActionResult> RejectFamilyLinkRequest(Guid requestId)
    {
        var result = await _mediator.Send(new RejectFamilyLinkRequestCommand(requestId));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy tất cả các yêu cầu liên kết liên quan đến một gia đình (gửi đi và nhận về).
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách các yêu cầu liên kết gia đình.</returns>
    [HttpGet("{familyId}")] // Route will be api/family-link-requests/{familyId}
    public async Task<ActionResult<List<FamilyLinkRequestDto>>> GetFamilyLinkRequests(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyLinkRequestsQuery(familyId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
