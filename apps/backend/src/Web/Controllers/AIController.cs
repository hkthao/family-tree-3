using backend.Application.AI.Commands.GenerateAiContent;
using backend.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using backend.Application.AI.Chat; // Add this using directive

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến AI.
/// </summary>
[Authorize]
[ApiController]
[Route("api/ai")]
[EnableRateLimiting(RateLimitConstants.PerUserPolicy)]
public class AiController(IMediator mediator, ILogger<AiController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AiController> _logger = logger;

    /// <summary>
    /// Tạo nội dung AI dựa trên đầu vào của người dùng và loại yêu cầu.
    /// </summary>
    /// <param name="command">Lệnh chứa FamilyId, ChatInput, và ContentType.</param>
    /// <returns>Nội dung được tạo bởi AI (JSON hoặc văn bản).</returns>
    [HttpPost("generate-content")]
    public async Task<IActionResult> GenerateAiContent([FromBody] GenerateAiContentCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Gửi tin nhắn đến AI Assistant và nhận phản hồi.
    /// </summary>
    /// <param name="command">Lệnh chứa FamilyId, SessionId, ChatInput và Metadata.</param>
    /// <returns>Phản hồi từ AI Assistant.</returns>
    [HttpPost("chat")]
    public async Task<IActionResult> ChatWithAssistant([FromBody] ChatWithAssistantCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }
}
