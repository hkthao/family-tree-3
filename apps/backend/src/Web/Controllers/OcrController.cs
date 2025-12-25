using backend.Application.Files.DTOs;
using Microsoft.AspNetCore.Mvc;
using backend.Application.OCR.Commands;

namespace backend.Web.Controllers;

/// <summary>
/// API Controller để xử lý các yêu cầu liên quan đến Dịch vụ OCR.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OcrController : ControllerBase // Changed to ControllerBase
{
    private readonly IMediator _mediator; // Inject Mediator
    private readonly ILogger<OcrController> _logger; // Inject Logger

    public OcrController(IMediator mediator, ILogger<OcrController> logger) // Constructor
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Thực hiện OCR trên một tệp hình ảnh hoặc PDF đã tải lên.
    /// </summary>
    /// <param name="file">Tệp hình ảnh hoặc PDF để xử lý.</param>
    /// <param name="lang">Ngôn ngữ để thực hiện OCR (mặc định: "eng+vie").</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Văn bản được OCR nếu thành công.</returns>
    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OcrResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessFileForOcr( // Changed return type to IActionResult
        IFormFile file,
        [FromQuery] string lang = "eng+vie",
        CancellationToken cancellationToken = default)
    {
        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream, cancellationToken);
            fileBytes = memoryStream.ToArray();
        }

        var command = new ProcessOcrFileCommand
        {
            FileBytes = fileBytes,
            ContentType = file.ContentType,
            FileName = file.FileName,
            Lang = lang
        };

        var result = await _mediator.Send(command, cancellationToken); // Use injected _mediator

        return result.ToActionResult(this, _logger); // Use ToActionResult extension method
    }
}
