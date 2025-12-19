using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.CreateEvents;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.SearchEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến sự kiện.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/event")]
public class EventController(IMediator mediator, ILogger<EventController> logger) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Đối tượng ILogger để ghi log.
    /// </summary>
    private readonly ILogger<EventController> _logger = logger;

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một sự kiện theo ID.
    /// </summary>
    /// <param name="id">ID của sự kiện cần lấy.</param>
    /// <returns>Thông tin chi tiết của sự kiện.</returns>
    [HttpGet("{id}", Name = "GetEventById")]
    public async Task<IActionResult> GetEventById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id));
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý POST request để tạo một sự kiện mới.
    /// </summary>
    /// <param name="command">Lệnh tạo sự kiện với thông tin chi tiết.</param>
    /// <returns>ID của sự kiện vừa được tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
    {
        if (command == null)
        {
            _logger.LogWarning("CreateEventCommand received a null command object from {RemoteIpAddress}", HttpContext.Connection.RemoteIpAddress);
            return BadRequest("Request body is empty or could not be deserialized into CreateEventCommand.");
        }
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 201, nameof(GetEventById), new { id = result.Value! });
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một sự kiện.
    /// </summary>
    /// <param name="id">ID của sự kiện cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật sự kiện với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            _logger.LogWarning("Mismatched ID in URL ({Id}) and request body ({CommandId}) for UpdateEventCommand from {RemoteIpAddress}", id, command.Id, HttpContext.Connection.RemoteIpAddress);
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một sự kiện.
    /// </summary>
    /// <param name="id">ID của sự kiện cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new DeleteEventCommand(id));
        return result.ToActionResult(this, _logger, 204);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm sự kiện dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các sự kiện tìm được.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách các sự kiện sắp tới cho người dùng hiện tại.
    /// </summary>
    /// <param name="familyId">Tùy chọn: Lọc sự kiện theo ID gia đình.</param>
    /// <returns>Danh sách các sự kiện sắp tới.</returns>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] Guid familyId)
    {
        var query = new Application.Events.Queries.GetUpcomingEvents.GetUpcomingEventsQuery
        {
            FamilyId = familyId,
        };
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều sự kiện cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều sự kiện với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các sự kiện vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<IActionResult> CreateEvents([FromBody] CreateEventsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }
}
