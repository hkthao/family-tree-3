using backend.Application.AI.Chat;
using backend.Application.AI.Commands;
using backend.Application.Families.Commands.GenerateFamilyData; // Updated import
using backend.Application.Families.DTOs; // New import for AnalyzedResultDto
using backend.Application.AI.DTOs; // UPDATED IMPORT
using backend.Application.AI.Models; // NEW IMPORT
using backend.Application.MemberStories.Commands.GenerateStory; // Updated
using backend.Application.MemberStories.DTOs; // Updated
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
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Tạo dữ liệu gia đình có cấu trúc bằng AI từ văn bản ngôn ngữ tự nhiên.
    /// </summary>
    /// <param name="command">Lệnh chứa văn bản cần phân tích và ID phiên làm việc.</param>
    /// <returns>Kết quả phân tích văn bản.</returns>
    [HttpPost("generate-family-data")]
    public async Task<ActionResult<AnalyzedResultDto>> GenerateFamilyData([FromBody] GenerateFamilyDataCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    /// <summary>
    /// Tạo câu chuyện bằng AI.
    /// </summary>
    /// <param name="command">Lệnh chứa thông tin cần thiết để tạo câu chuyện.</param>
    /// <returns>Câu chuyện đã tạo.</returns>
    [HttpPost("generate-story")]
    public async Task<ActionResult<GenerateStoryResponseDto>> GenerateStory([FromBody] GenerateStoryCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }
}
