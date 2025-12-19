using backend.Application.AI.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến AI.
/// </summary>
[Authorize]
[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AIController> _logger;

    public AIController(IMediator mediator, ILogger<AIController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gửi một tin nhắn đến AI Assistant và nhận phản hồi.
    /// </summary>
    /// <param name="command">Lệnh chứa tin nhắn và lịch sử trò chuyện.</param>
    /// <returns>Phản hồi từ AI Assistant.</returns>
    [HttpPost("chat")]
    public async Task<IActionResult> ChatWithAssistant([FromBody] ChatWithAssistantCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }
}
