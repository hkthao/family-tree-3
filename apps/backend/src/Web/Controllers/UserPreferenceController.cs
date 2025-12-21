using backend.Application.UserPreferences.Commands.SaveUserPreferences;
using backend.Application.UserPreferences.Queries.GetUserPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/user-preference")]
[EnableRateLimiting(RateLimitConstants.UserPolicy)]
/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến tùy chọn người dùng.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
public class UserPreferenceController(IMediator mediator, ILogger<UserPreferenceController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<UserPreferenceController> _logger = logger;

    /// <summary>
    /// Truy xuất tùy chọn của người dùng hiện tại.
    /// </summary>
    /// <returns>Tùy chọn của người dùng.</returns>
    [HttpGet]
    public async Task<IActionResult> GetUserPreferences()
    {
        var result = await _mediator.Send(new GetUserPreferencesQuery());
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Lưu tùy chọn của người dùng hiện tại.
    /// </summary>
    /// <param name="command">Lệnh chứa dữ liệu tùy chọn người dùng.</param>
    /// <returns>Một đối tượng Result cho biết thành công hay thất bại.</returns>
    [HttpPut]
    public async Task<IActionResult> SaveUserPreferences([FromBody] SaveUserPreferencesCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }
}
