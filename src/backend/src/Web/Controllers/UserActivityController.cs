using backend.Application.UserActivities.Queries;
using backend.Application.UserActivities.Queries.GetRecentActivities;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/activity")]
/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến hoạt động của người dùng.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
public class UserActivityController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Truy xuất danh sách các hoạt động gần đây của người dùng hiện tại.
    /// </summary>
    /// <param name="limit">Số lượng hoạt động tối đa để trả về (mặc định: 20).</param>
    /// <param name="targetType">Tùy chọn: Lọc hoạt động theo loại tài nguyên mục tiêu.</param>
    /// <param name="targetId">Tùy chọn: Lọc hoạt động theo ID của tài nguyên mục tiêu.</param>
    /// <param name="groupId">Tùy chọn: Lọc hoạt động theo ID nhóm (FamilyId).</param>
    /// <returns>Danh sách các hoạt động gần đây của người dùng.</returns>
    [HttpGet("recent")]
    public async Task<ActionResult<List<UserActivityDto>>> GetRecentActivities(
        [FromQuery] int limit = 20,
        [FromQuery] TargetType? targetType = null,
        [FromQuery] string? targetId = null,
        [FromQuery] Guid? groupId = null)
    {
        var query = new GetRecentActivitiesQuery
        {
            Limit = limit,
            TargetType = targetType,
            TargetId = targetId,
            GroupId = groupId
        };

        var result = await _mediator.Send(query);

        return result.IsSuccess ? (ActionResult<List<UserActivityDto>>)Ok(result.Value) : (ActionResult<List<UserActivityDto>>)BadRequest(result.Error);
    }
}
