using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob;
using backend.Application.ImageRestorationJobs.Commands.DeleteImageRestorationJob;
using backend.Application.ImageRestorationJobs.Commands.UpdateImageRestorationJob;
using backend.Application.ImageRestorationJobs.Queries.GetImageRestorationJobById;
using backend.Application.ImageRestorationJobs.Queries.GetImageRestorationJobs;
using backend.Application.ImageRestorationJobs.Queries.SearchImageRestorationJobs;
using backend.Application.ImageRestorationJobs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/image-restoration-jobs")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class ImageRestorationJobsController(IMediator mediator, ILogger<ImageRestorationJobsController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<ImageRestorationJobsController> _logger = logger;

    /// <summary>
    /// Lấy danh sách các job phục hồi ảnh của người dùng hiện tại.
    /// </summary>
    /// <returns>Danh sách các ImageRestorationJobDto.</returns>
    [HttpGet]
    public async Task<IActionResult> GetImageRestorationJobs()
    {
        var result = await _mediator.Send(new GetImageRestorationJobsQuery());
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tìm kiếm và phân trang các job phục hồi ảnh của người dùng hiện tại.
    /// </summary>
    /// <param name="query">Query chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>PaginatedList các ImageRestorationJobDto.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<ImageRestorationJobDto>>> SearchImageRestorationJobs([FromQuery] SearchImageRestorationJobsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một job phục hồi ảnh theo ID.
    /// </summary>
    /// <param name="id">ID của job phục hồi ảnh.</param>
    /// <returns>ImageRestorationJobDto.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetImageRestorationJobById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetImageRestorationJobByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tạo một job phục hồi ảnh mới.
    /// </summary>
    /// <param name="imageFile">File ảnh cần phục hồi.</param>
    /// <param name="familyId">ID của gia đình liên quan.</param>
    /// <param name="useCodeformer">Sử dụng CodeFormer để phục hồi ảnh (mặc định là false).</param>
    /// <returns>ImageRestorationJobDto của job đã tạo.</returns>
    [HttpPost]
    [Consumes("multipart/form-data")] // Specify content type for file uploads
    public async Task<IActionResult> CreateImageRestorationJob(
        IFormFile imageFile,
        [FromForm] Guid familyId,
        [FromForm] bool useCodeformer = false)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return BadRequest("Image file is required.");
        }

        // Read the image file into a byte array
        byte[] imageData;
        using (var memoryStream = new MemoryStream())
        {
            await imageFile.CopyToAsync(memoryStream);
            imageData = memoryStream.ToArray();
        }

        var command = new CreateImageRestorationJobCommand(
            ImageData: imageData,
            FileName: imageFile.FileName,
            ContentType: imageFile.ContentType,
            FamilyId: familyId,
            UseCodeformer: useCodeformer
        );

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201); // 201 Created
    }

    /// <summary>
    /// Cập nhật thông tin của một job phục hồi ảnh.
    /// </summary>
    /// <param name="id">ID của job phục hồi ảnh cần cập nhật.</param>
    /// <param name="command">Dữ liệu cập nhật.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateImageRestorationJob([FromRoute] Guid id, [FromBody] UpdateImageRestorationJobCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest($"Id in route ({id}) does not match Id in body ({command.Id}).");
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful update
    }

    /// <summary>
    /// Cập nhật trạng thái của một job phục hồi ảnh.
    /// </summary>
    /// <param name="id">ID của job phục hồi ảnh cần cập nhật trạng thái.</param>
    /// <param name="command">Dữ liệu cập nhật trạng thái.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPatch("{id}/status")]
    [AllowAnonymous] // Allow external service to call back without authentication
    public async Task<IActionResult> UpdateImageRestorationJobStatus([FromRoute] Guid id, [FromBody] UpdateImageRestorationJobCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest($"Id in route ({id}) does not match Id in body ({command.Id}).");
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful status update
    }

    /// <summary>
    /// Xóa một job phục hồi ảnh.
    /// </summary>
    /// <param name="id">ID của job phục hồi ảnh cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImageRestorationJob([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeleteImageRestorationJobCommand(id));
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful deletion
    }
}
