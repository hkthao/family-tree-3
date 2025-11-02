using backend.Application.Common.Models;
using backend.Application.Events;
using backend.Application.Events.Commands.CreateEvent;
using backend.Application.Events.Commands.CreateEvents;
using backend.Application.Events.Commands.DeleteEvent;
using backend.Application.Events.Commands.GenerateEventData;
using backend.Application.Events.Commands.UpdateEvent;
using backend.Application.Events.Queries;
using backend.Application.Events.Queries.GetEventById;
using backend.Application.Events.Queries.GetEvents;
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
public class EventController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để lấy danh sách các sự kiện.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa các tiêu chí lọc và phân trang.</param>
    /// <returns>Danh sách các sự kiện.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetEvents([FromQuery] GetEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<IReadOnlyList<EventDto>>)Ok(result.Value) : (ActionResult<IReadOnlyList<EventDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một sự kiện theo ID.
    /// </summary>
    /// <param name="id">ID của sự kiện cần lấy.</param>
    /// <returns>Thông tin chi tiết của sự kiện.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEventById([FromRoute]Guid id)
    {
        var result = await _mediator.Send(new GetEventByIdQuery(id));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error); // Assuming NotFound for single item retrieval failure
    }

    /// <summary>
    /// Xử lý POST request để tạo một sự kiện mới.
    /// </summary>
    /// <param name="command">Lệnh tạo sự kiện với thông tin chi tiết.</param>
    /// <returns>ID của sự kiện vừa được tạo.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody]CreateEventCommand command)
    {
        if (command == null)
        {
            return BadRequest("Request body is empty or could not be deserialized into CreateEventCommand.");
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<Guid>)CreatedAtAction(nameof(GetEventById), new { id = result.Value }, result.Value) : (ActionResult<Guid>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một sự kiện.
    /// </summary>
    /// <param name="id">ID của sự kiện cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật sự kiện với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute]Guid id, [FromBody]UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một sự kiện.
    /// </summary>
    /// <param name="id">ID của sự kiện cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute]Guid id)
    {
        var result = await _mediator.Send(new DeleteEventCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm sự kiện dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các sự kiện tìm được.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<EventDto>>> Search([FromQuery] SearchEventsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<PaginatedList<EventDto>>)Ok(result.Value) : (ActionResult<PaginatedList<EventDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy danh sách các sự kiện sắp tới cho người dùng hiện tại.
    /// </summary>
    /// <param name="familyId">Tùy chọn: Lọc sự kiện theo ID gia đình.</param>
    /// <returns>Danh sách các sự kiện sắp tới.</returns>
    [HttpGet("upcoming")]
    public async Task<ActionResult<List<EventDto>>> GetUpcomingEvents([FromQuery] Guid? familyId = null)
    {
        var query = new backend.Application.Events.Queries.GetUpcomingEvents.GetUpcomingEventsQuery
        {
            FamilyId = familyId,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };
        var result = await _mediator.Send(query);
        return result.IsSuccess ? (ActionResult<List<EventDto>>)Ok(result.Value) : (ActionResult<List<EventDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo dữ liệu sự kiện mẫu.
    /// </summary>
    /// <param name="command">Lệnh tạo dữ liệu sự kiện.</param>
    /// <returns>Danh sách các sự kiện được tạo.</returns>
    [HttpPost("generate-event-data")]
    public async Task<ActionResult<List<AIEventDto>>> GenerateEventData([FromBody] GenerateEventDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<AIEventDto>>)Ok(result.Value) : (ActionResult<List<AIEventDto>>)BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo nhiều sự kiện cùng lúc.
    /// </summary>
    /// <param name="command">Lệnh tạo nhiều sự kiện với danh sách thông tin chi tiết.</param>
    /// <returns>Danh sách ID của các sự kiện vừa được tạo.</returns>
    [HttpPost("bulk-create")]
    public async Task<ActionResult<List<Guid>>> CreateEvents([FromBody] CreateEventsCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<List<Guid>>)Ok(result.Value) : (ActionResult<List<Guid>>)BadRequest(result.Error);
    }
}
