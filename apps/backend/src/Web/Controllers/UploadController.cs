using backend.Application.Common.Models;
using backend.Application.Files.Queries.GetUploadedFile;
using backend.Application.Files.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO; // Required for MemoryStream
using System.Threading; // Required for CancellationToken

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến tải lên và truy xuất tệp.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/upload")]
public class UploadController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Tải lên một tệp lên nhà cung cấp lưu trữ đã cấu hình.
    /// </summary>
    /// <param name="file">Tệp cần tải lên.</param>
    /// <param name="cloud">Dịch vụ lưu trữ đám mây (e.g., "imgbb").</param>
    /// <param name="folder">Thư mục trong lưu trữ đám mây.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa URL của tệp đã tải lên nếu thành công, hoặc lỗi nếu thất bại.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<string>>> Upload([FromForm] IFormFile file, [FromQuery] string? cloud, [FromQuery] string? folder, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        byte[] imageData;
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream, cancellationToken);
            imageData = memoryStream.ToArray();
        }

        var command = new UploadFileCommand
        {
            ImageData = imageData,
            FileName = file.FileName,
            Cloud = cloud ?? "imgbb", // Use query param or default
            Folder = folder ?? "family-tree-memories" // Use query param or default
        };
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess ? (ActionResult<Result<string>>)Ok(result) : (ActionResult<Result<string>>)BadRequest(result);
    }

    // Removed GetUploadedFile endpoint as it is no longer relevant with the new external storage approach
    /*
    /// <summary>
    /// Truy xuất một tệp đã tải lên để xem trước, yêu cầu xác thực.
    /// </summary>
    /// <param name="fileName">Tên của tệp cần truy xuất.</param>
    /// <returns>Nội dung tệp hoặc 404 Not Found nếu không tìm thấy.</returns>
    [HttpGet("preview/{fileName}")]
    public async Task<IActionResult> GetUploadedFile(string fileName)
    {
        var query = new GetUploadedFileQuery { FileName = fileName };
        var result = await _mediator.Send(query);

        return result.IsSuccess && result.Value != null
            ? File(result.Value.Content, result.Value.ContentType)
            : result.ErrorSource == "NotFound" ? NotFound() : BadRequest(result.Error);
    }
    */
}
