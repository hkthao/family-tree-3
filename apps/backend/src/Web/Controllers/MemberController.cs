using backend.Application.Common.Models;
using backend.Application.Members.Commands.CreateMember;
using backend.Application.Members.Commands.CreateMembers;
using backend.Application.Members.Commands.DeleteMember;
using backend.Application.Members.Commands.UpdateMember;
using backend.Application.Members.Commands.UpdateMemberBiography;
using backend.Application.Members.Queries.GetMemberById;
using backend.Application.Members.Queries.GetMembers;
using backend.Application.Members.Queries.GetMembersByFamilyId;
using backend.Application.Members.Queries.GetMembersByIds;
using backend.Application.Members.Queries.SearchMembers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using backend.Application.Common.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến thành viên gia đình.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
/// <param name="logger">Đối tượng ILogger để ghi log.</param>
[Authorize]
[ApiController]
[Route("api/member")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class MemberController(IMediator mediator, ILogger<MemberController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;
    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<MemberController> _logger = logger;

    /// <summary>
    /// Xử lý GET request để tìm kiếm thành viên dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các thành viên tìm được.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchMembersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một thành viên theo ID.
    /// </summary>
    /// <param name="id">ID của thành viên cần lấy.</param>
    /// <returns>Thông tin chi tiết của thành viên.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMemberById(Guid id)
    {
        var result = await _mediator.Send(new GetMemberByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách thành viên theo nhiều ID.
    /// </summary>
    /// <param name="ids">Chuỗi chứa các ID thành viên, phân tách bằng dấu phẩy.</param>
    /// <returns>Danh sách các thành viên.</returns>
    [HttpGet("by-ids")]
    public async Task<IActionResult> GetMembersByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
        {
            return Result<List<MemberListDto>>.Success([]).ToActionResult(this, _logger);
        }

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetMembersByIdsQuery(guids));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách thành viên theo ID gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình cần lấy thành viên.</param>
    /// <returns>Danh sách các thành viên thuộc gia đình.</returns>
    [HttpGet("by-family/{familyId}")]
    public async Task<IActionResult> GetMembersByFamilyId(Guid familyId)
    {
        var result = await _mediator.Send(new GetMembersByFamilyIdQuery(familyId));
        return result.ToActionResult(this, _logger);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetMemberById), new { id = result.Value });
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều thành viên cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều thành viên với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các thành viên vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> CreateMembers([FromBody] CreateMembersCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
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
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateMemberCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
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
        return result.ToActionResult(this, _logger, 204);
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
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandMemberId}) for UpdateMemberBiographyCommand from {RemoteIpAddress}", id, command.MemberId, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }
}
