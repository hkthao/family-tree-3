using backend.Application.Common.Models;
using backend.Application.Faces.Commands.DetectFaces;
using backend.Application.Faces.Commands.SaveFaceLabels;
using backend.Application.Faces.Queries;
using backend.Application.Faces.Queries.GetDetectedFaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;
[ApiController]
[Route("api/[controller]")]
public class FacesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("detect")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FaceDetectionResponseDto>> DetectFaces([FromForm] IFormFile file, [FromQuery] bool returnCrop = true)
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
            ReturnCrop = returnCrop
        };

        return await _mediator.Send(command);
    }

    [HttpGet("detected/{imageId}")]
    public async Task<ActionResult<List<DetectedFaceDto>>> GetDetectedFaces(Guid imageId)
    {
        return await _mediator.Send(new GetDetectedFacesQuery { ImageId = imageId });
    }

    [HttpPost("labels")]
    public async Task<ActionResult<Result<Unit>>> SaveFaceLabels([FromBody] SaveFaceLabelsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Result<Unit>>)Ok(result) : (ActionResult<Result<Unit>>)BadRequest(result.Error);
    }
}
