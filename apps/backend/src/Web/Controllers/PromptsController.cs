using backend.Application.Prompts.Commands.CreatePrompt;
using backend.Application.Prompts.Commands.DeletePrompt;
using backend.Application.Prompts.Commands.UpdatePrompt;
using backend.Application.Prompts.Queries.GetPromptById;
using backend.Application.Prompts.Queries.SearchPrompts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến lời nhắc (prompt).
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[Authorize]
[ApiController]
[Route("api/prompts")]
public class PromptsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một lời nhắc theo ID hoặc Code.
    /// </summary>
    /// <param name="id">ID của lời nhắc cần lấy (tùy chọn).</param>
    /// <param name="code">Mã của lời nhắc cần lấy (tùy chọn).</param>
    /// <returns>Thông tin chi tiết của lời nhắc.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPromptById([FromRoute] Guid id)
    {
        var result = await _mediator.Send(new GetPromptByIdQuery { Id = id });
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để lấy thông tin chi tiết của một lời nhắc theo Code.
    /// </summary>
    /// <param name="code">Mã của lời nhắc cần lấy.</param>
    /// <returns>Thông tin chi tiết của lời nhắc.</returns>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetPromptByCode([FromRoute] string code)
    {
        var result = await _mediator.Send(new GetPromptByIdQuery { Code = code });
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Xử lý GET request để tìm kiếm lời nhắc dựa trên các tiêu chí được cung cấp.
    /// </summary>
    /// <param name="query">Đối tượng chứa các tiêu chí tìm kiếm và phân trang.</param>
    /// <returns>Một PaginatedList chứa danh sách các lời nhắc tìm được.</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchPrompts([FromQuery] SearchPromptsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý POST request để tạo một lời nhắc mới.
    /// </summary>
    /// <param name="command">Lệnh tạo lời nhắc với thông tin chi tiết.</param>
    /// <returns>ID của lời nhắc vừa được tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePrompt([FromBody] CreatePromptCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetPromptById), new { id = result.Value }, result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý PUT request để cập nhật thông tin của một lời nhắc.
    /// </summary>
    /// <param name="id">ID của lời nhắc cần cập nhật.</param>
    /// <param name="command">Lệnh cập nhật lời nhắc với thông tin mới.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePrompt([FromRoute] Guid id, [FromBody] UpdatePromptCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }
        var result = await _mediator.Send(command);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Xử lý DELETE request để xóa một lời nhắc.
    /// </summary>
    /// <param name="id">ID của lời nhắc cần xóa.</param>
    /// <returns>IActionResult cho biết kết quả của thao tác.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePrompt(Guid id)
    {
        var result = await _mediator.Send(new DeletePromptCommand(id));
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
