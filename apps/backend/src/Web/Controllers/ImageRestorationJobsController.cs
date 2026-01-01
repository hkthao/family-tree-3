using backend.Application.Common.Constants;
using backend.Application.Common.Extensions;
using backend.Application.Common.Models; // Added for PaginatedList
using backend.Application.ImageRestorationJobs.Commands.CreateImageRestorationJob; // Added
using backend.Application.ImageRestorationJobs.Commands.DeleteImageRestorationJob;
using backend.Application.ImageRestorationJobs.Commands.UpdateImageRestorationJob;
using backend.Application.ImageRestorationJobs.Queries.GetImageRestorationJobById;
using backend.Application.ImageRestorationJobs.Queries.GetImageRestorationJobs;
using backend.Application.ImageRestorationJobs.Queries.SearchImageRestorationJobs; // Added
using backend.Application.ImageRestorationJobs.Common; // Added
using MediatR;
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
    /// <param name="jobId">ID của job phục hồi ảnh.</param>
    /// <returns>ImageRestorationJobDto.</returns>
    [HttpGet("{jobId}")]
    public async Task<IActionResult> GetImageRestorationJobById([FromRoute] string jobId)
    {
        var result = await _mediator.Send(new GetImageRestorationJobByIdQuery(jobId));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tạo một job phục hồi ảnh mới.
    /// </summary>
    /// <param name="command">Dữ liệu để tạo job phục hồi ảnh.</param>
    /// <returns>ImageRestorationJobDto của job đã tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateImageRestorationJob([FromBody] CreateImageRestorationJobCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201); // 201 Created
    }

    /// <summary>
    /// Cập nhật thông tin của một job phục hồi ảnh.
    /// </summary>
    /// <param name="jobId">ID của job phục hồi ảnh cần cập nhật.</param>
    /// <param name="command">Dữ liệu cập nhật.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{jobId}")]
    public async Task<IActionResult> UpdateImageRestorationJob([FromRoute] string jobId, [FromBody] UpdateImageRestorationJobCommand command)
    {
        if (jobId != command.JobId)
        {
            return BadRequest($"JobId in route ({jobId}) does not match JobId in body ({command.JobId}).");
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful update
    }

    /// <summary>
    /// Xóa một job phục hồi ảnh.
    /// </summary>
    /// <param name="jobId">ID của job phục hồi ảnh cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPatch("{jobId}/status")]
    [AllowAnonymous] // Allow external service to call back without authentication
    public async Task<IActionResult> UpdateImageRestorationJobStatus([FromRoute] string jobId, [FromBody] UpdateImageRestorationJobCommand command)
    {
        if (jobId != command.JobId)
        {
            return BadRequest($"JobId in route ({jobId}) does not match JobId in body ({command.JobId}).");
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful status update
    }

    /// <summary>
    /// Xóa một job phục hồi ảnh.
    /// </summary>
    /// <param name="jobId">ID của job phục hồi ảnh cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{jobId}")]
    public async Task<IActionResult> DeleteImageRestorationJob([FromRoute] string jobId)
    {
        var result = await _mediator.Send(new DeleteImageRestorationJobCommand(jobId));
        return result.ToActionResult(this, _logger, 204); // 204 No Content for successful deletion
    }
}
