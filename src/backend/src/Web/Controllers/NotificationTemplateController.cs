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

[Authorize]
[ApiController]
[Route("api/notification-template")]
public class NotificationTemplateController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PaginatedList<NotificationTemplateDto>>> GetNotificationTemplates([FromQuery] GetNotificationTemplatesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationTemplateDto>> GetNotificationTemplateById(Guid id)
    {
        var result = await _mediator.Send(new GetNotificationTemplateByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

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

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotificationTemplate(Guid id)
    {
        var result = await _mediator.Send(new DeleteNotificationTemplateCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
