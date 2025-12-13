using backend.Application.Common.Constants; // Added to resolve ErrorSources
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.MemberFaces.Commands.UpdateMemberFace;
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.MemberFaces.Queries.SearchMemberFaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/member-faces")]
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

    [HttpPost]
    public async Task<IActionResult> CreateMemberFace(CreateMemberFaceCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetMemberFaceById), new { id = result.Value });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMemberFace(Guid id, UpdateMemberFaceCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateMemberFaceCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return Result<Unit>.Failure("Mismatched ID in URL and request body.", ErrorSources.Validation).ToActionResult(this, _logger);
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
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
}
