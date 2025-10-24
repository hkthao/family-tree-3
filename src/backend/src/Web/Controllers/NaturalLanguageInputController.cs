using Application.NaturalLanguageInput.Commands.GenerateData;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến đầu vào ngôn ngữ tự nhiên.
/// </summary>
/// <param name="mediator">Đối tượng IMediator để gửi các lệnh và truy vấn.</param>
[ApiController]
[Route("api/[controller]")]
public class NaturalLanguageInputController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng IMediator để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Xử lý POST request để tạo dữ liệu sự kiện từ đầu vào ngôn ngữ tự nhiên.
    /// </summary>
    /// <param name="command">Lệnh tạo dữ liệu sự kiện.</param>
    /// <returns>Dữ liệu sự kiện được tạo dưới dạng chuỗi JSON.</returns>
    [HttpPost("generate-event-data")]
    public async Task<ActionResult<string>> GenerateEventData([FromBody] GenerateEventDataCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
