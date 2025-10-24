using backend.Application.Common.Models;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.Members.Commands.GenerateMemberData;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.Members.Commands.UpdateMemberBiography;
using backend.Application.Members.Commands.GenerateBiography;
using backend.Application.Members.Queries;
using backend.Application.Members.Queries.GetEditableMembers;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMembersByIds;
using backend.Application.Members.Queries.SearchMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến thành viên gia đình.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
/// <param name="logger">Đối tượng ILogger để ghi log.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MembersController(IMediator mediator, ILogger<MembersController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;
    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<MembersController> _logger = logger;

    /// <summary>
    /// Tạo tiểu sử cho một thành viên bằng AI.
    /// </summary>
    /// <param name="id">ID của thành viên.</param>
    /// <param name="command">Lệnh tạo tiểu sử.</param>
    /// <returns>Nội dung tiểu sử được tạo và siêu dữ liệu.</returns>
    [HttpPost("{id}/biography/generate")]
    public async Task<ActionResult<BiographyResultDto>> GenerateBiography(Guid id, [FromBody] GenerateBiographyCommand command)
    {
        if (id != command.MemberId)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<BiographyResultDto>)Ok(result.Value) : (ActionResult<BiographyResultDto>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm thành viên dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các thành viên tìm được.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<MemberListDto>>> Search([FromQuery] SearchMembersQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error); // Or other appropriate error handling
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một thành viên theo ID.
    /// </summary>
    /// <param name="id">ID của thành viên cần lấy.</param>
    /// <returns>Thông tin chi tiết của thành viên.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDetailDto>> GetMemberById(Guid id)
    {
        var result = await _mediator.Send(new GetMemberByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error); // Assuming NotFound for single item retrieval failure
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách thành viên theo nhiều ID.
    /// </summary>
    /// <param name="ids">Chuỗi chứa các ID thành viên, phân tách bằng dấu phẩy.</param>
    /// <returns>Danh sách các thành viên.</returns>
    [HttpGet("by-ids")]
    public async Task<ActionResult<List<MemberListDto>>> GetMembersByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return Ok(Result<List<MemberListDto>>.Success([]).Value);
        }

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetMembersByIdsQuery(guids));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error); // Or other appropriate error handling
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách các thành viên mà người dùng hiện tại có quyền chỉnh sửa.
    /// </summary>
    /// <returns>Danh sách các thành viên có thể chỉnh sửa.</returns>
    [HttpGet("managed")]
    public async Task<ActionResult<List<MemberListDto>>> GetEditableMembers()
    {
        var result = await _mediator.Send(new GetEditableMembersQuery());
        return result.IsSuccess ? (ActionResult<List<MemberListDto>>)Ok(result.Value) : (ActionResult<List<MemberListDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo một thành viên mới.
    /// </summary>
    /// <param name="command">Lệnh tạo thành viên với thông tin chi tiết.</param>
    /// <returns>ID của thành viên vừa được tạo.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateMember([FromBody] CreateMemberCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Guid>)CreatedAtAction(nameof(GetMemberById), new { id = result.Value }, result.Value) : (ActionResult<Guid>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo dữ liệu thành viên mẫu.
    /// </summary>
    /// <param name="command">Lệnh tạo dữ liệu thành viên.</param>
    /// <returns>Danh sách các thành viên được tạo.</returns>
    [HttpPost("generate-member-data")]
    public async Task<ActionResult<List<MemberDto>>> GenerateMemberData([FromBody] GenerateMemberDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<MemberDto>>)Ok(result.Value) : (ActionResult<List<MemberDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều thành viên cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều thành viên với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các thành viên vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateMembers([FromBody] CreateMembersCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<Guid>>)Ok(result.Value) : (ActionResult<List<Guid>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một thành viên.
    /// </summary>
    /// <param name="id">ID của thành viên cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật thành viên với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMember(Guid id, [FromBody] UpdateMemberCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một thành viên.
    /// </summary>
    /// <param name="id">ID của thành viên cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMember(Guid id)
    {
        var result = await _mediator.Send(new DeleteMemberCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật tiểu sử của một thành viên.
    /// </summary>
    /// <param name="id">ID của thành viên cần cập nhật tiểu sử.</param>
    /// <param name="command">Lệnh cập nhật tiểu sử thành viên với nội dung mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}/biography")]
    public async Task<IActionResult> UpdateMemberBiography(Guid id, [FromBody] UpdateMemberBiographyCommand command)
    {
        if (id != command.MemberId)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
