using backend.Application.Common.Models;
using backend.Application.Files.Queries.GetUploadedFile;
using backend.Application.Files.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

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

        return result.IsSuccess ? (ActionResult<Result<string>>)Ok(result) : (ActionResult<Result<string>>)BadRequest(result);
    }

    /// <summary>
    /// Retrieves an uploaded file for preview, requiring authentication.
    /// </summary>
    /// <param name="fileName">The name of the file to retrieve.</param>
    /// <returns>The file content or a 404 Not Found.</returns>
    [HttpGet("preview/{fileName}")]
    public async Task<IActionResult> GetUploadedFile(string fileName)
    {
        var query = new GetUploadedFileQuery { FileName = fileName };
        var result = await _mediator.Send(query);

        return result.IsSuccess && result.Value != null
            ? File(result.Value.Content, result.Value.ContentType)
            : result.ErrorSource == "NotFound" ? NotFound() : BadRequest(result.Error);
    }
}
