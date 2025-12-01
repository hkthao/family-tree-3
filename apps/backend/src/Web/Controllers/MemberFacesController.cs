using backend.Application.Common.Constants; // Added to resolve ErrorSources
using backend.Application.Common.Models;
using backend.Application.MemberFaces.Commands.CreateMemberFace;
using backend.Application.MemberFaces.Commands.DeleteMemberFace;
using backend.Application.MemberFaces.Commands.DetectFaces;
using backend.Application.MemberFaces.Commands.UpdateMemberFace;
using backend.Application.MemberFaces.Common; // For MemberFaceDto
using backend.Application.MemberFaces.Queries.GetMemberFaceById;
using backend.Application.MemberFaces.Queries.SearchMemberFaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/memberfaces")]
public class MemberFacesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<Result<PaginatedList<MemberFaceDto>>>> SearchMemberFaces([FromQuery] SearchMemberFacesQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<MemberFaceDto>>> GetMemberFaceById(Guid id)
    {
        return await _mediator.Send(new GetMemberFaceByIdQuery { Id = id });
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateMemberFace(CreateMemberFaceCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result<Unit>>> UpdateMemberFace(Guid id, UpdateMemberFaceCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(Result<Unit>.Failure("Mismatched ID in URL and request body.", ErrorSources.Validation));
        }
        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result<Unit>>> DeleteMemberFace(Guid id)
    {
        return await _mediator.Send(new DeleteMemberFaceCommand { Id = id });
    }

    /// <summary>
    /// Xử lý POST request để phát hiện khuôn mặt trong một hình ảnh.
    /// </summary>
    /// <param name="file">Tệp hình ảnh cần xử lý.</param>
    /// <param name="returnCrop">Có trả về ảnh cắt của khuôn mặt đã phát hiện hay không.</param>
    /// <returns>Đối tượng chứa thông tin về các khuôn mặt đã phát hiện.</returns>
    [HttpPost("detect")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FaceDetectionResponseDto>> DetectFaces([FromForm] IFormFile file, [FromQuery] bool resizeImageForAnalysis = false, [FromQuery] bool returnCrop = true)
    {
        if (file == null || file.Length == 0)
        {
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
            ResizeImageForAnalysis = resizeImageForAnalysis
        };

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
