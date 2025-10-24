using backend.Application.Dashboard.Queries;
using backend.Application.Dashboard.Queries.GetDashboardStats;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến bảng điều khiển (dashboard).
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để lấy các số liệu thống kê cho bảng điều khiển của người dùng hiện tại.
    /// </summary>
    /// <param name="familyId">Tùy chọn: Lọc số liệu thống kê theo ID gia đình.</param>
    /// <returns>Các số liệu thống kê của bảng điều khiển.</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats([FromQuery] Guid? familyId = null)
    {
        var query = new GetDashboardStatsQuery { FamilyId = familyId };
        var result = await _mediator.Send(query);

        return result.IsSuccess ? (ActionResult<DashboardStatsDto>)Ok(result.Value) : (ActionResult<DashboardStatsDto>)BadRequest(result.Error);
    }
}
