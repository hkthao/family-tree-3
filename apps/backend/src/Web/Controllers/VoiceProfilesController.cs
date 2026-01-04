using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.VoiceGenerations.Commands.GenerateVoice;
using backend.Application.VoiceGenerations.Queries.GetVoiceGenerationHistory;
using backend.Application.VoiceProfiles.Commands.PreprocessAndCreateVoiceProfile;
using backend.Application.VoiceProfiles.Commands.ImportVoiceProfiles; // Add this using statement
using backend.Application.VoiceProfiles.Commands.DeleteVoiceProfile;
using backend.Application.VoiceProfiles.Queries.GetVoiceProfileById;
using backend.Application.VoiceProfiles.Queries.SearchVoiceProfiles;
using backend.Application.VoiceProfiles.Queries.ExportVoiceProfiles; // Add this using statement
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến hồ sơ giọng nói (Voice Profiles) và việc tạo giọng nói (Voice Generations).
/// </summary>
[Authorize]
[ApiController]
[Route("api/voice-profiles")] // Route base for the controller
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class VoiceProfilesController(IMediator mediator, ILogger<VoiceProfilesController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<VoiceProfilesController> _logger = logger;

    /// <summary>
    /// Lấy thông tin chi tiết của một hồ sơ giọng nói.
    /// </summary>
    /// <param name="id">ID của hồ sơ giọng nói.</param>
    /// <returns>Thông tin chi tiết hồ sơ giọng nói.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVoiceProfileById(Guid id)
    {
        Result<backend.Application.VoiceProfiles.Queries.VoiceProfileDto> result = await _mediator.Send(new GetVoiceProfileByIdQuery { Id = id });
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Cập nhật thông tin của một hồ sơ giọng nói.
    /// </summary>
    /// <param name="id">ID của hồ sơ giọng nói cần cập nhật.</param>
    /// <param name="command">Dữ liệu để cập nhật hồ sơ giọng nói.</param>
    /// <returns>Hồ sơ giọng nói đã được cập nhật.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVoiceProfile(Guid id, [FromBody] UpdateVoiceProfileCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateVoiceProfileCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest("ID trong URL và trong body không khớp.");
        }
        Result<backend.Application.VoiceProfiles.Queries.VoiceProfileDto> result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 200); // 200 OK for successful update returning entity
    }

    /// <summary>
    /// Xóa một hồ sơ giọng nói.
    /// </summary>
    /// <param name="id">ID của hồ sơ giọng nói cần xóa.</param>
    /// <returns>Kết quả của hoạt động xóa.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVoiceProfile(Guid id)
    {
        var result = await _mediator.Send(new DeleteVoiceProfileCommand(id));
        return result.ToActionResult(this, this._logger, 204); // 204 No Content for successful deletion
    }

    /// <summary>
    /// Tạo giọng nói từ một hồ sơ giọng nói và văn bản.
    /// </summary>
    /// <param name="id">ID của hồ sơ giọng nói sẽ được sử dụng.</param>
    /// <param name="command">Dữ liệu để tạo giọng nói.</param>
    /// <returns>Thông tin về quá trình tạo giọng nói.</returns>
    [HttpPost("{id}/generate")]
    public async Task<IActionResult> GenerateVoice(Guid id, [FromBody] GenerateVoiceCommand command)
    {
        if (id != command.VoiceProfileId)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandVoiceProfileId}) for GenerateVoiceCommand from {RemoteIpAddress}", id, command.VoiceProfileId, HttpContext.Connection.RemoteIpAddress);
            return BadRequest("ID trong URL và trong body không khớp.");
        }
        Result<Application.VoiceGenerations.Queries.VoiceGenerationDto> result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201);
    }

    /// <summary>
    /// Lấy lịch sử tạo giọng nói của một hồ sơ giọng nói.
    /// </summary>
    /// <param name="id">ID của hồ sơ giọng nói.</param>
    /// <returns>Danh sách lịch sử tạo giọng nói.</returns>
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetVoiceGenerationHistory(Guid id)
    {
        var result = await _mediator.Send(new GetVoiceGenerationHistoryQuery { VoiceProfileId = id });
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tìm kiếm các hồ sơ giọng nói dựa trên các tiêu chí tìm kiếm.
    /// </summary>
    /// <param name="query">Các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Danh sách hồ sơ giọng nói đã được phân trang.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<backend.Application.VoiceProfiles.Queries.VoiceProfileDto>>> SearchVoiceProfiles([FromQuery] SearchVoiceProfilesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Tiền xử lý các file audio thô và tạo một hồ sơ giọng nói mới cho một thành viên.
    /// </summary>
    /// <param name="command">Dữ liệu để tiền xử lý audio và tạo hồ sơ giọng nói.</param>
    /// <returns>Hồ sơ giọng nói vừa được tạo.</returns>
    [HttpPost("preprocess-and-create")]
    public async Task<IActionResult> PreprocessAndCreateVoiceProfile([FromBody] PreprocessAndCreateVoiceProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetVoiceProfileById), result.IsSuccess ? new { id = result.Value!.Id } : null);
    }

    /// <summary>
    /// Xuất danh sách các hồ sơ giọng nói của một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách các hồ sơ giọng nói.</returns>
    [HttpGet("{familyId}/export")]
    public async Task<IActionResult> ExportVoiceProfiles(Guid familyId)
    {
        var result = await _mediator.Send(new ExportVoiceProfilesQuery { FamilyId = familyId });
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Nhập danh sách các hồ sơ giọng nói vào một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <param name="command">Dữ liệu để nhập hồ sơ giọng nói.</param>
    /// <returns>Kết quả của quá trình nhập.</returns>
    [HttpPost("{familyId}/import")]
    public async Task<IActionResult> ImportVoiceProfiles(Guid familyId, [FromBody] ImportVoiceProfilesCommand command)
    {
        if (familyId != command.FamilyId)
        {
            _logger.LogWarning("Mismatched Family ID in URL ({FamilyId}) and request body ({CommandFamilyId}) for ImportVoiceProfilesCommand from {RemoteIpAddress}", familyId, command.FamilyId, HttpContext.Connection.RemoteIpAddress);
            return BadRequest("Family ID trong URL và trong body không khớp.");
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 200);
    }
}
