using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using backend.Application.Identity.Queries;
using backend.Application.Identity.Queries.GetUserByUsernameOrEmail; // Added
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến người dùng.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/user")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class UserController(IMediator mediator, ILogger<UserController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<UserController> _logger = logger;

    /// <summary>
    /// Xử lý GET request để tìm kiếm người dùng dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách người dùng tìm được.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách người dùng theo nhiều ID.
    /// </summary>
    /// <param name="ids">Chuỗi chứa các ID người dùng, phân tách bằng dấu phẩy.</param>
    /// <returns>Danh sách các đối tượng UserDto.</returns>
    [HttpGet("by-ids")]
    public async Task<IActionResult> GetUsersByIds([FromQuery] string ids)
    {
        if (string.IsNullOrEmpty(ids))
            return Result<List<UserDto>>.Success([]).ToActionResult(this, _logger);

        var guids = ids.Split(',').Select(Guid.Parse).ToList();
        var result = await _mediator.Send(new GetUsersByIdsQuery(guids));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm một người dùng dựa trên tên người dùng hoặc email.
    /// </summary>
    /// <param name="usernameOrEmail">Tên người dùng hoặc email để tìm kiếm.</param>
    /// <returns>Thông tin chi tiết của người dùng nếu tìm thấy.</returns>
    [HttpGet("find")]
    public async Task<IActionResult> FindUser([FromQuery] string usernameOrEmail)
    {
        var result = await _mediator.Send(new GetUserByUsernameOrEmailQuery { UsernameOrEmail = usernameOrEmail });
        return result.ToActionResult(this, _logger);
    }
}
