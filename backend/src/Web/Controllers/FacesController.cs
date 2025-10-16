namespace FamilyTree.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using FamilyTree.Application.Faces.Commands.DetectFaces;
using FamilyTree.Application.Faces.Commands;
using FamilyTree.Application.Faces.Queries;
using FamilyTree.Application.Faces.Queries.GetDetectedFaces;
using FamilyTree.Application.Faces.Commands.LabelFace;
using MediatR;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class FacesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FacesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("detect")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<FaceDetectionResultDto>>> DetectFaces([FromForm] IFormFile file, [FromQuery] bool returnCrop = true)
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

    [HttpPost("label")]
    public async Task<ActionResult> LabelFace([FromBody] LabelFaceCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
