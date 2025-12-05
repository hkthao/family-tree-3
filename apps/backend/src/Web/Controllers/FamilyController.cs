using backend.Application.Common.Models;
using backend.Application.Families.Commands.CreateFamilies;
using backend.Application.Families.Commands.CreateFamily;
using backend.Application.Families.Commands.DeleteFamily;
using backend.Application.Families.Commands.GenerateFamilyData;
using backend.Application.Families.Commands.UpdateFamily;
using backend.Application.Families.Queries;
using backend.Application.Families.Queries.GetFamiliesByIds;
using backend.Application.Families.Queries.GetFamilyById;
using backend.Application.Families.Queries.GetUserFamilyAccessQuery;
using backend.Application.Families.Queries.SearchFamilies;
using backend.Application.Members.Commands.UpdateDenormalizedFields;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến gia đình.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/family")]
public class FamilyController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Lấy danh sách các gia đình mà người dùng hiện tại có quyền truy cập (Manager hoặc Viewer).
    /// </summary>
    /// <returns>Danh sách các đối tượng FamilyAccessDto.</returns>
    [HttpGet("my-access")]
    public async Task<ActionResult<List<FamilyAccessDto>>> GetUserFamilyAccess()
    {
        var result = await _mediator.Send(new GetUserFamilyAccessQuery());
        return Ok(result);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một gia đình theo ID.
    /// </summary>
    /// <param name="id">ID của gia đình cần lấy.</param>
    /// <returns>Thông tin chi tiết của gia đình.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FamilyDto>> GetFamilyById(Guid id)
    {
        var result = await _mediator.Send(new GetFamilyByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error); // Assuming NotFound for single item retrieval failure
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm gia đình dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các gia đình tìm được.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<FamilyDto>>> Search([FromQuery] SearchFamiliesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<FamilyDto>>)Ok(result.Value) : (ActionResult<PaginatedList<FamilyDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo một gia đình mới.
    /// </summary>
    /// <param name="command">Lệnh tạo gia đình với thông tin chi tiết.</param>
    /// <returns>ID của gia đình vừa được tạo.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateFamily([FromBody] CreateFamilyCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Guid>)CreatedAtAction(nameof(GetFamilyById), new { id = result.Value }, result.Value) : (ActionResult<Guid>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều gia đình cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều gia đình với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các gia đình vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateFamilies([FromBody] CreateFamiliesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<Guid>>)Ok(result.Value) : (ActionResult<List<Guid>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một gia đình.
    /// </summary>
    /// <param name="id">ID của gia đình cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật gia đình với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFamily([FromRoute] Guid id, [FromBody] UpdateFamilyCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một gia đình.
    /// </summary>
    /// <param name="id">ID của gia đình cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFamily(Guid id)
    {
        var result = await _mediator.Send(new DeleteFamilyCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách các gia đình theo nhiều ID.
    /// </summary>
    /// <param name="ids">Chuỗi chứa các ID gia đình, phân tách bằng dấu phẩy.</param>
    /// <returns>Danh sách các đối tượng FamilyDto.</returns>
    [HttpGet("by-ids")]
    public async Task<IActionResult> GetFamiliesByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return Ok(Result<List<FamilyDto>>.Success([]).Value);

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetFamiliesByIdsQuery(guids));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Cập nhật các trường mối quan hệ đã chuẩn hóa cho tất cả các thành viên trong một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần cập nhật.</param>
    /// <returns>Kết quả của hoạt động cập nhật.</returns>
    [HttpPost("{familyId}/update-denormalized-fields")]
    public async Task<ActionResult> UpdateDenormalizedFields([FromRoute] Guid familyId)
    {
        var result = await _mediator.Send(new UpdateDenormalizedFieldsCommand(familyId));
        return result.IsSuccess ? Ok("Denormalized relationship fields updated successfully for the family.") : BadRequest(result.Error);
    }

    /// <summary>
    /// Tạo dữ liệu gia đình có cấu trúc bằng AI từ văn bản ngôn ngữ tự nhiên.
    /// </summary>
    /// <param name="command">Lệnh chứa văn bản cần phân tích và ID phiên làm việc.</param>
    /// <returns>Kết quả phân tích văn bản.</returns>
    [HttpPost("generate-data")]
    public async Task<IActionResult> GenerateFamilyData([FromBody] GenerateFamilyDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }
}
