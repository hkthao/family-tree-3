using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Application.Relationships.Queries.DetectRelationship;
using backend.Application.Relationships.Queries.GetRelationshipById;
using backend.Application.Relationships.Queries.GetRelationships;
using backend.Application.Relationships.Queries.SearchRelationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using backend.Application.Common.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến mối quan hệ giữa các thành viên.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/relationship")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class RelationshipController(IMediator mediator, ILogger<RelationshipController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<RelationshipController> _logger = logger;

    /// <summary>
    /// Xử lý GET request để lấy danh sách các mối quan hệ.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRelationships([FromQuery] GetRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một mối quan hệ theo ID.
    /// </summary>
    /// <param name="id">ID của mối quan hệ cần lấy.</param>
    /// <returns>Thông tin chi tiết của mối quan hệ.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRelationshipById(Guid id)
    {
        var result = await _mediator.Send(new GetRelationshipByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm mối quan hệ dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các mối quan hệ tìm được.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để phát hiện mối quan hệ giữa hai thành viên.
    /// </summary>
    /// <param name="familyId">ID của gia đình mà các thành viên thuộc về.</param>
    /// <param name="memberAId">ID của thành viên A.</param>
    /// <param name="memberBId">ID của thành viên B.</param>
    /// <returns>Kết quả phát hiện mối quan hệ.</returns>
    [HttpGet("detect-relationship")]
    public async Task<IActionResult> DetectRelationship([FromQuery] Guid familyId, [FromQuery] Guid memberAId, [FromQuery] Guid memberBId)
    {
        var result = await _mediator.Send(new DetectRelationshipQuery(familyId, memberAId, memberBId));
        return result.Description != "unknown" ? Ok(result) : NotFound(result); // Leave as is
    }

    /// <summary>
    /// Xử lý POST request để tạo một mối quan hệ mới.
    /// </summary>
    /// <param name="command">Lệnh tạo mối quan hệ với thông tin chi tiết.</param>
    /// <returns>ID của mối quan hệ vừa được tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateRelationship([FromBody] CreateRelationshipCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetRelationshipById), new { id = result.Value });
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều mối quan hệ cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều mối quan hệ với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các mối quan hệ vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> CreateRelationships([FromBody] CreateRelationshipsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một mối quan hệ.
    /// </summary>
    /// <param name="id">ID của mối quan hệ cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật mối quan hệ với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRelationship(Guid id, [FromBody] UpdateRelationshipCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateRelationshipCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một mối quan hệ.
    /// </summary>
    /// <param name="id">ID của mối quan hệ cần xóa.</param>
    /// <returns>NoContent nếu xóa thành công.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelationship(Guid id)
    {
        var result = await _mediator.Send(new DeleteRelationshipCommand(id));
        return result.ToActionResult(this, _logger, 204);
    }
}
