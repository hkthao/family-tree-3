using backend.Application.Common.Models;
using backend.Application.Files.Queries.GetUploadedFile;
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
[Route("api/[controller]")]
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
    /// <param name="cancellationToken">Token hủy bỏ thao tác.</param>
    /// <returns>Một đối tượng Result chứa URL của tệp đã tải lên nếu thành công, hoặc lỗi nếu thất bại.</returns>
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
}
