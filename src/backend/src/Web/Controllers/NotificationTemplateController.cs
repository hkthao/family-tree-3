using backend.Application.Common.Models;
using backend.Application.NotificationTemplates.Commands.CreateNotificationTemplate;
using backend.Application.NotificationTemplates.Commands.DeleteNotificationTemplate;
using backend.Application.NotificationTemplates.Commands.UpdateNotificationTemplate;
using backend.Application.NotificationTemplates.Commands.GenerateNotificationTemplateContent;
using backend.Application.NotificationTemplates.Queries;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplateById;
using backend.Application.NotificationTemplates.Queries.GetNotificationTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến mẫu thông báo.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/notification-template")]
public class NotificationTemplateController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để lấy danh sách các mẫu thông báo.
    /// </summary>
    /// <param name="query">Đối tượng truy vấn chứa các tiêu chí lọc và phân trang.</param>
    /// <returns>Danh sách các mẫu thông báo.</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedList<NotificationTemplateDto>>> GetNotificationTemplates([FromQuery] GetNotificationTemplatesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một mẫu thông báo theo ID.
    /// </summary>
    /// <param name="id">ID của mẫu thông báo cần lấy.</param>
    /// <returns>Thông tin chi tiết của mẫu thông báo.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationTemplateDto>> GetNotificationTemplateById(Guid id)
    {
        var result = await _mediator.Send(new GetNotificationTemplateByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo một mẫu thông báo mới.
    /// </summary>
    /// <param name="command">Lệnh tạo mẫu thông báo với thông tin chi tiết.</param>
    /// <returns>ID của mẫu thông báo vừa được tạo.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateNotificationTemplate([FromBody] CreateNotificationTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetNotificationTemplateById), new { id = result.Value }, result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Tạo nội dung và chủ đề cho mẫu thông báo bằng AI.
    /// </summary>
    /// <param name="command">Lệnh tạo nội dung mẫu thông báo bằng AI.</param>
    /// <returns>Nội dung và chủ đề được tạo bởi AI.</returns>
    [HttpPost("generate-content-ai")]
    public async Task<ActionResult<GeneratedNotificationTemplateContentDto>> GenerateNotificationTemplateContent([FromBody] GenerateNotificationTemplateContentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một mẫu thông báo.
    /// </summary>
    /// <param name="id">ID của mẫu thông báo cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật mẫu thông báo với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateNotificationTemplate(Guid id, [FromBody] UpdateNotificationTemplateCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một mẫu thông báo.
    /// </summary>
    /// <param name="id">ID của mẫu thông báo cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotificationTemplate(Guid id)
    {
        var result = await _mediator.Send(new DeleteNotificationTemplateCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
