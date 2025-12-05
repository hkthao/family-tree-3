using backend.Application.AI.Chat;
using backend.Application.AI.Commands;
using backend.Application.MemberStories.Commands.GenerateStory; // Updated
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

    public AIController(IMediator mediator)
    {
        _mediator = mediator;
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
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Tạo tiểu sử cho một thành viên bằng AI.
    /// </summary>
    /// <param name="command">Lệnh chứa thông tin thành viên và các tùy chọn tạo tiểu sử.</param>
    /// <returns>Tiểu sử đã tạo.</returns>
    [HttpPost("biography")]
    public async Task<IActionResult> GenerateBiography([FromBody] GenerateBiographyCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Tạo câu chuyện bằng AI.
    /// </summary>
    /// <param name="command">Lệnh chứa thông tin cần thiết để tạo câu chuyện.</param>
    /// <returns>Câu chuyện đã tạo.</returns>
    [HttpPost("generate-story")]
    public async Task<IActionResult> GenerateStory([FromBody] GenerateStoryCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
