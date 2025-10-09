using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.Common.Interfaces;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;
    private readonly IStorageSettings _storageSettings;

    public UploadController(IMediator mediator, IWebHostEnvironment env, IStorageSettings storageSettings)
    {
        _mediator = mediator;
        _env = env;
        _storageSettings = storageSettings;
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

    /// <summary>
    /// Retrieves an uploaded file for preview, requiring authentication.
    /// </summary>
    /// <param name="fileName">The name of the file to retrieve.</param>
    /// <returns>The file content or a 404 Not Found.</returns>
    [HttpGet("preview/{fileName}")]
    public IActionResult GetUploadedFile(string fileName)
    {
        // Sanitize fileName to prevent path traversal
        var sanitizedFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(_env.WebRootPath, _storageSettings.LocalStoragePath, sanitizedFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        // Determine content type
        var contentType = "application/octet-stream"; // Default
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        switch (ext)
        {
            case ".jpg":
            case ".jpeg":
                contentType = "image/jpeg";
                break;
            case ".png":
                contentType = "image/png";
                break;
            case ".pdf":
                contentType = "application/pdf";
                break;
            case ".docx":
                contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                break;
        }

        return PhysicalFile(filePath, contentType);
    }
}
