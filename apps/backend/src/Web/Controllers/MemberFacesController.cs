using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.MemberFaces.Commands.ImportMemberFaces;
using backend.Application.MemberFaces.Commands.SyncMemberFacesToFaceService; // NEW
using backend.Application.MemberFaces.Queries.ExportMemberFaces;
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.MemberFaces.Queries.GetMemberFacesByMemberId;
using backend.Application.MemberFaces.Queries.SearchMemberFaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[Authorize] // Added explicit Authorize attribute
[ApiController]
[Route("api/member-faces")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class MemberFacesController(IMediator mediator, ILogger<MemberFacesController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<MemberFacesController> _logger = logger;

    [HttpGet("search")]
    public async Task<IActionResult> SearchMemberFaces([FromQuery] SearchMemberFacesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMemberFaceById(Guid id)
    {
        var result = await _mediator.Send(new GetMemberFaceByIdQuery { Id = id });
        return result.ToActionResult(this, _logger);
    }

    [HttpGet("by-member/{memberId}")]
    public async Task<IActionResult> GetMemberFacesByMemberId(Guid memberId)
    {
        var result = await _mediator.Send(new GetMemberFacesByMemberIdQuery { MemberId = memberId });
        return result.ToActionResult(this, _logger);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMemberFace(CreateMemberFaceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetMemberFaceById), new { id = result.Value });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMemberFace(Guid id)
    {
        var result = await _mediator.Send(new DeleteMemberFaceCommand { Id = id });
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý POST request để phát hiện khuôn mặt trong một hình ảnh.
    /// </summary>
    /// <param name="file">Tệp hình ảnh cần xử lý.</param>
    /// <param name="returnCrop">Có trả về ảnh cắt của khuôn mặt đã phát hiện hay không.</param>
    /// <returns>Đối tượng chứa thông tin về các khuôn mặt đã phát hiện.</returns>
    [HttpPost("detect")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> DetectFaces([FromForm] IFormFile file, Guid familyId, [FromQuery] bool resizeImageForAnalysis = false, [FromQuery] bool returnCrop = true)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("DetectFaces received no image file from {RemoteIpAddress}", HttpContext.Connection.RemoteIpAddress);
            return BadRequest("No image file uploaded.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        var command = new DetectFacesCommand
        {
            ImageBytes = imageBytes,
            ContentType = file.ContentType,
            ReturnCrop = returnCrop,
            FamilyId = familyId,
            ResizeImageForAnalysis = resizeImageForAnalysis
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xuất tất cả khuôn mặt thành viên cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách khuôn mặt thành viên.</returns>
    [HttpGet("export")] // Changed route
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportMemberFaces([FromQuery] Guid? familyId) // Changed to FromQuery and optional
    {
        if (!familyId.HasValue)
        {
            return BadRequest(Result.Failure("familyId is required for export if not provided in the route.", ErrorSources.Validation));
        }
        var result = await _mediator.Send(new ExportMemberFacesQuery(familyId.Value));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Nhập danh sách khuôn mặt thành viên cho một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="command">Lệnh nhập khuôn mặt thành viên với thông tin chi tiết.</param>
    /// <returns>Danh sách khuôn mặt thành viên vừa được nhập.</returns>
    [HttpPost("import")] // Changed route
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportMemberFaces([FromBody] ImportMemberFacesCommand command) // Removed familyId from path
    {
        // Validation check for familyId is now done within the command handler or implicitly by model binding
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201);
    }

    /// <summary>
    /// Đồng bộ hóa dữ liệu khuôn mặt thành viên với knowledge service.
    /// </summary>
    /// <param name="command">Lệnh đồng bộ hóa với các tùy chọn như FamilyId và ForceResyncAll.</param>
    /// <returns>Kết quả của quá trình đồng bộ hóa.</returns>
    [HttpPost("sync-to-face-service")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // For unauthorized (non-admin) access
    public async Task<IActionResult> SyncMemberFacesToFaceService([FromBody] SyncMemberFacesToFaceServiceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }
}
