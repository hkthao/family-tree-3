using backend.Application.Common.Models;
using backend.Application.Identity.UserProfiles.Commands.UpdateUserProfile;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Application.Identity.UserProfiles.Queries.GetAllUserProfiles;
using backend.Application.Identity.UserProfiles.Queries.GetCurrentUserProfile;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileByExternalId;
using backend.Application.Identity.UserProfiles.Queries.GetUserProfileById;
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
public class UserProfilesController(IMediator mediator) : ControllerBase
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
    /// Xử lý GET request để lấy tất cả hồ sơ người dùng.
    /// </summary>
    /// <returns>Danh sách tất cả hồ sơ người dùng.</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserProfileDto>>> GetAllUserProfiles()
    {
        var result = await _mediator.Send(new GetAllUserProfilesQuery());
        return result.IsSuccess ? (ActionResult<List<UserProfileDto>>)Ok(result.Value) : (ActionResult<List<UserProfileDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy hồ sơ người dùng theo ID bên ngoài.
    /// </summary>
    /// <param name="externalId">ID bên ngoài của người dùng.</param>
    /// <returns>Hồ sơ người dùng.</returns>
    [HttpGet("byExternalId/{externalId}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfileByExternalId(string externalId)
    {
        var result = await _mediator.Send(new GetUserProfileByExternalIdQuery { ExternalId = externalId });
        return result.IsSuccess ? (ActionResult<UserProfileDto>)Ok(result.Value) : (ActionResult<UserProfileDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy hồ sơ người dùng theo ID nội bộ.
    /// </summary>
    /// <param name="id">ID nội bộ của người dùng.</param>
    /// <returns>Hồ sơ người dùng.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfileById(Guid id)
    {
        var result = await _mediator.Send(new GetUserProfileByIdQuery { Id = id });
        return result.IsSuccess ? (ActionResult<UserProfileDto>)Ok(result.Value) : (ActionResult<UserProfileDto>)NotFound(result.Error);
    }

    /// <summary>
    /// Cập nhật hồ sơ của người dùng hiện tại.
    /// </summary>
    /// <param name="userId">ID của người dùng cần cập nhật.</param>
    /// <param name="command">Lệnh chứa dữ liệu hồ sơ người dùng đã cập nhật.</param>
    /// <returns>Một đối tượng Result cho biết thành công hay thất bại.</returns>
    [HttpPut("{userId}")]
    public async Task<ActionResult<Result>> UpdateUserProfile(string userId, [FromBody] UpdateUserProfileCommand command)
    {
        if (userId != command.Id)
        {
            return BadRequest(Result.Failure("User ID in URL must match user ID in request body.", "BadRequest"));
        }

        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Result>)Ok(result) : (ActionResult<Result>)BadRequest(result);
    }
}
