using backend.Application.Common.Models;
using backend.Application.Relationships.Commands.CreateRelationship;
using backend.Application.Relationships.Commands.CreateRelationships;
using backend.Application.Relationships.Commands.DeleteRelationship;
using backend.Application.Relationships.Commands.GenerateRelationshipData;
using backend.Application.Relationships.Commands.UpdateRelationship;
using backend.Application.Relationships.Queries;
using backend.Application.Relationships.Queries.GetRelationshipById;
using backend.Application.Relationships.Queries.GetRelationships;
using backend.Application.Relationships.Queries.SearchRelationships;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến mối quan hệ giữa các thành viên.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RelationshipsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để lấy danh sách các mối quan hệ.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa các tiêu chí lọc và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các mối quan hệ.</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedList<RelationshipListDto>>> GetRelationships([FromQuery] GetRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<RelationshipListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<RelationshipListDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một mối quan hệ theo ID.
    /// </summary>
    /// <param name="id">ID của mối quan hệ cần lấy.</param>
    /// <returns>Thông tin chi tiết của mối quan hệ.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<RelationshipDto>> GetRelationshipById(Guid id)
    {
        var result = await _mediator.Send(new GetRelationshipByIdQuery(id));
        return result.IsSuccess ? (ActionResult<RelationshipDto>)Ok(result.Value) : (ActionResult<RelationshipDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm mối quan hệ dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các mối quan hệ tìm được.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<RelationshipListDto>>> Search([FromQuery] SearchRelationshipsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<RelationshipListDto>>)Ok(result.Value) : (ActionResult<PaginatedList<RelationshipListDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo một mối quan hệ mới.
    /// </summary>
    /// <param name="command">Lệnh tạo mối quan hệ với thông tin chi tiết.</param>
    /// <returns>ID của mối quan hệ vừa được tạo.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRelationship([FromBody] CreateRelationshipCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess
            ? (ActionResult<Guid>)CreatedAtAction(nameof(GetRelationshipById), new { id = result.Value }, result.Value)
            : (ActionResult<Guid>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo dữ liệu mối quan hệ mẫu.
    /// </summary>
    /// <param name="command">Lệnh tạo dữ liệu mối quan hệ.</param>
    /// <returns>Danh sách các mối quan hệ được tạo.</returns>
    [HttpPost("generate-relationship-data")]
    public async Task<ActionResult<List<RelationshipDto>>> GenerateRelationshipData([FromBody] GenerateRelationshipDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<RelationshipDto>>)Ok(result.Value) : (ActionResult<List<RelationshipDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều mối quan hệ cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều mối quan hệ với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các mối quan hệ vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateRelationships([FromBody] CreateRelationshipsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<Guid>>)Ok(result.Value) : (ActionResult<List<Guid>>)BadRequest(result.Error);
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
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một mối quan hệ.
    /// </summary>
    /// <param name="id">ID của mối quan hệ cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelationship(Guid id)
    {
        var result = await _mediator.Send(new DeleteRelationshipCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
