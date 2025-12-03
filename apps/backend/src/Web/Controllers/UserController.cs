using backend.Application.Common.Models;
using backend.Application.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến người dùng.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/user")]
public class UserController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để tìm kiếm người dùng dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách người dùng tìm được.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<UserDto>>> Search([FromQuery] SearchUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<UserDto>>)Ok(result.Value) : (ActionResult<PaginatedList<UserDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách người dùng theo nhiều ID.
    /// </summary>
    /// <param name="ids">Chuỗi chứa các ID người dùng, phân tách bằng dấu phẩy.</param>
    /// <returns>Danh sách các đối tượng UserDto.</returns>
    [HttpGet("by-ids")]
    public async Task<ActionResult<List<UserDto>>> GetUsersByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return Ok(Result<List<UserDto>>.Success([]).Value);

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetUsersByIdsQuery(guids));
        return result.IsSuccess ? (ActionResult<List<UserDto>>)Ok(result.Value) : (ActionResult<List<UserDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Checks if the current user is an admin.
    /// </summary>
    /// <returns>True if the user is an admin, otherwise false.</returns>
    [HttpGet("IsAdmin")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsAdmin() // Change to async Task<IActionResult>
    {
        var result = await _mediator.Send(new IsAdminQuery());
        return result.IsSuccess ? (ActionResult)Ok(result.Value) : (ActionResult)BadRequest(result.Error);
    }

    /// <summary>
    /// Checks if the current user has manager privileges for a specific family.
    /// </summary>
    /// <param name="familyId">The ID of the family to check.</param>
    /// <returns>True if the user can manage the family, otherwise false.</returns>
    [HttpGet("IsFamilyManager/{familyId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsFamilyManager(Guid familyId) // Change to async Task<IActionResult>
    {
        var result = await _mediator.Send(new IsFamilyManagerQuery(familyId));
        return result.IsSuccess ? (ActionResult)Ok(result.Value) : (ActionResult)BadRequest(result.Error);
    }
}

