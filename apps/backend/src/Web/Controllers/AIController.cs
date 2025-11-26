using backend.Application.AI.Chat;
using backend.Application.AI.Commands;
using backend.Application.AI.Commands.AnalyzeNaturalLanguage; // NEW IMPORT
using backend.Application.AI.Commands.AnalyzePhoto; // UPDATED IMPORT
using backend.Application.AI.DTOs; // UPDATED IMPORT
using backend.Application.AI.Models; // NEW IMPORT
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
        return result.IsSuccess ? Ok(result) : BadRequest(result);
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
    /// Phân tích một bức ảnh bằng AI để trích xuất thông tin bối cảnh và cảm xúc.
    /// </summary>
    /// <param name="command">Lệnh chứa dữ liệu ảnh và các tham số phân tích.</param>
    /// <returns>Kết quả phân tích ảnh.</returns>
    [HttpPost("analyze-photo")]
    public async Task<ActionResult<PhotoAnalysisResultDto>> AnalyzePhoto([FromBody] AnalyzePhotoCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }

    /// <summary>
    /// Phân tích văn bản ngôn ngữ tự nhiên bằng AI để trích xuất thông tin về thành viên, sự kiện, mối quan hệ.
    /// </summary>
    /// <param name="command">Lệnh chứa văn bản cần phân tích và ID phiên làm việc.</param>
    /// <returns>Kết quả phân tích văn bản.</returns>
    [HttpPost("analyze-natural-language")]
    public async Task<ActionResult<AnalyzedResultDto>> AnalyzeNaturalLanguage([FromBody] AnalyzeNaturalLanguageCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result);
    }
}
