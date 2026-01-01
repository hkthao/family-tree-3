using backend.Application.Common.Constants;
using backend.Application.Common.Extensions; // NEW
using backend.Application.Common.Models.ImageRestoration; // NEW
using backend.Application.FamilyMedias.Commands.StartImageRestoration;
using backend.Application.FamilyMedias.Queries.GetImageRestorationStatus;
using backend.Application.FamilyMedias.Commands.CreateFamilyMedia;
using backend.Application.FamilyMedias.Commands.DeleteFamilyMedia;
using backend.Application.FamilyMedias.Queries.GetFamilyMediaById;
using backend.Application.FamilyMedias.Queries.SearchFamilyMedia;
using backend.Web.Models.FamilyMedia; // NEW
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-media")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class FamilyMediaController(IMediator mediator, ILogger<FamilyMediaController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FamilyMediaController> _logger = logger;



    /// <summary>
    /// Tải lên một file media mới cho một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="request">Request chứa thông tin file media và IFormFile.</param>
    /// <returns>ID của file media vừa được tạo.</returns>
    [HttpPost("{familyId}")]
    [Consumes("multipart/form-data")] // Specify content type for file uploads
    public async Task<IActionResult> CreateFamilyMedia([FromRoute] Guid familyId, [FromForm] CreateFamilyMediaRequest request)
    {
        if (request.File == null)
        {
            return BadRequest("No file uploaded.");
        }

        byte[] fileBytes;
        using (var memoryStream = new MemoryStream())
        {
            await request.File.CopyToAsync(memoryStream);
            fileBytes = memoryStream.ToArray();
        }

        var command = new CreateFamilyMediaCommand
        {
            FamilyId = familyId,
            File = fileBytes,
            FileName = request.File.FileName,
            MediaType = request.File.FileName.InferMediaType(),
            Description = request.Description,
            Folder = request.Folder
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetFamilyMediaById), new { familyId, id = result.Value });
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một file media theo ID.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="id">ID của file media cần lấy.</param>
    /// <returns>Thông tin chi tiết của file media.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFamilyMediaById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetFamilyMediaByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Lấy danh sách các file media cho một gia đình.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchFamilyMedia([FromQuery] SearchFamilyMediaQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xóa một file media.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="id">ID của file media cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamilyMedia([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyMediaCommand { Id = id });
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Tạo media mới từ URL cho một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="request">Request chứa thông tin URL media.</param>
    /// <returns>ID của media vừa được tạo.</returns>
    [HttpPost("{familyId}/from-url")]
    public async Task<IActionResult> CreateFamilyMediaFromUrl([FromRoute] Guid familyId, [FromBody] CreateFamilyMediaFromUrlRequest request)
    {
        var command = new Application.FamilyMedias.Commands.CreateFamilyMediaFromUrl.CreateFamilyMediaFromUrlCommand
        {
            FamilyId = familyId,
            Url = request.Url,
            FileName = request.FileName,
            MediaType = request.MediaType,
            Description = request.Description,
            Folder = request.Folder
        };

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetFamilyMediaById), new { familyId, id = result.Value });
    }

    /// <summary>
    /// Bắt đầu một tiến trình phục hồi ảnh mới.
    /// </summary>
    /// <param name="request">Request chứa URL ảnh gốc.</param>
    /// <returns>Job ID và trạng thái ban đầu.</returns>
    [HttpPost("restore-image")]
    public async Task<IActionResult> StartImageRestoration([FromBody] StartImageRestorationRequestDto request)
    {
        var result = await _mediator.Send(new StartImageRestorationCommand(request.ImageUrl));
        return result.ToActionResult(this, _logger, 202); // 202 Accepted for asynchronous processing
    }

    /// <summary>
    /// Lấy trạng thái của một tiến trình phục hồi ảnh.
    /// </summary>
    /// <param name="jobId">ID của job phục hồi ảnh.</param>
    /// <returns>Trạng thái và kết quả của job.</returns>
    [HttpGet("restoration-status/{jobId}")]
    public async Task<IActionResult> GetImageRestorationStatus([FromRoute] Guid jobId)
    {
        var result = await _mediator.Send(new GetImageRestorationStatusCommand(jobId));
        return result.ToActionResult(this, _logger);
    }
}

