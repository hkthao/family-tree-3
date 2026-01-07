using backend.Application.Common.Models;
using backend.Application.FamilyFollows; // Added for FamilyFollowDto
using backend.Application.FamilyFollows.Commands.FollowFamily;
using backend.Application.FamilyFollows.Commands.UnfollowFamily;
using backend.Application.FamilyFollows.Commands.UpdateFamilyFollowSettings;
using backend.Application.FamilyFollows.Queries.GetFamilyFollowers;
using backend.Application.FamilyFollows.Queries.GetFollowStatus;
using backend.Application.FamilyFollows.Queries.GetUserFollowedFamilies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/family-follows")]
public class FamilyFollowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FamilyFollowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cho phép người dùng hiện tại theo dõi một gia đình.
    /// </summary>
    /// <param name="command">Thông tin yêu cầu theo dõi gia đình.</param>
    /// <returns>ID của đối tượng FamilyFollow đã tạo nếu thành công.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Result<Guid>>> FollowFamily([FromBody] FollowFamilyCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cho phép người dùng hiện tại bỏ theo dõi một gia đình.
    /// </summary>
    /// <param name="familyId">ID của gia đình muốn bỏ theo dõi.</param>
    /// <returns>Kết quả thành công nếu bỏ theo dõi thành công.</returns>
    [HttpDelete("{familyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> UnfollowFamily(Guid familyId)
    {
        var command = new UnfollowFamilyCommand { FamilyId = familyId };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Cập nhật cài đặt thông báo theo dõi gia đình cho người dùng hiện tại.
    /// </summary>
    /// <param name="familyId">ID của gia đình muốn cập nhật cài đặt.</param>
    /// <param name="command">Thông tin cài đặt thông báo mới.</param>
    /// <returns>Kết quả thành công nếu cập nhật thành công.</returns>
    [HttpPut("{familyId}/preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> UpdateFamilyFollowPreferences(Guid familyId, [FromBody] UpdateFamilyFollowSettingsCommand command)
    {
        if (familyId != command.FamilyId)
        {
            return BadRequest(Result.Failure("FamilyId in route does not match FamilyId in body."));
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy danh sách các gia đình mà người dùng hiện tại đang theo dõi.
    /// </summary>
    /// <returns>Danh sách các FamilyFollowDto.</returns>
    [HttpGet("my-follows")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<ICollection<FamilyFollowDto>>>> GetMyFollowedFamilies()
    {
        var result = await _mediator.Send(new GetUserFollowedFamiliesQuery());
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy trạng thái theo dõi của người dùng hiện tại đối với một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>FamilyFollowDto nếu đang theo dõi, hoặc lỗi nếu không tìm thấy.</returns>
    [HttpGet("{familyId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<FamilyFollowDto>>> GetFollowStatus(Guid familyId)
    {
        var result = await _mediator.Send(new GetFollowStatusQuery { FamilyId = familyId });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Lấy danh sách những người theo dõi một gia đình cụ thể.
    /// </summary>
    /// <param name="familyId">ID của gia đình.</param>
    /// <returns>Danh sách các FamilyFollowDto của những người theo dõi.</returns>
    [HttpGet("{familyId}/followers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Result<ICollection<FamilyFollowDto>>>> GetFamilyFollowers(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyFollowersQuery { FamilyId = familyId });
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
