using backend.Application.Common.Models;
using backend.Application.Files.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// <param name="folder">Thư mục trong lưu trữ đám mây.</param>
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa URL của tệp đã tải lên nếu thành công, hoặc lỗi nếu thất bại.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Result<string>>> Upload([FromForm] IFormFile file, [FromQuery] string? folder, CancellationToken cancellationToken)
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
            ContentType = file.ContentType,
            Folder = folder ?? "family-tree-memories" // Use query param or default
        };
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess ? (ActionResult<Result<string>>)Ok(result) : (ActionResult<Result<string>>)BadRequest(result);
    }
}
