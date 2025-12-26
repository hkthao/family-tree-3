using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Images.Commands.UploadImage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload-image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageUploadResultDto>> UploadImage([FromForm] UploadImageCommand command)
    {
        // MediatR will handle the command and return the result
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
