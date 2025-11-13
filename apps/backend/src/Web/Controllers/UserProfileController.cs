using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/user-profile")]
/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến hồ sơ người dùng.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
public class UserProfileController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để lấy hồ sơ của người dùng hiện tại.
    /// </summary>
    /// <returns>Hồ sơ của người dùng hiện tại.</returns>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
    {
        var result = await _mediator.Send(new GetCurrentUserProfileQuery());
        return result.IsSuccess ? (ActionResult<UserProfileDto>)Ok(result.Value) : (ActionResult<UserProfileDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Cập nhật hồ sơ của người dùng hiện tại.
    /// </summary>
    /// <param name="userId">ID của người dùng cần cập nhật.</param>
    /// <param name="command">Lệnh chứa dữ liệu hồ sơ người dùng đã cập nhật.</param>
    /// <returns>Một đối tượng Result cho biết thành công hay thất bại.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> UpdateUserProfile([FromRoute] Guid id, [FromBody] UpdateUserProfileCommand command)
    {
        command.SetId(id);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Result>)Ok(result) : (ActionResult<Result>)BadRequest(result);
    }
}
