using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;

    public UploadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Uploads a file to the configured storage provider.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the URL of the uploaded file on success, or an error on failure.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<string>>> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var command = new UploadFileCommand
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
