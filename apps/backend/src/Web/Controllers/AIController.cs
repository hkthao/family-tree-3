using backend.Application.AI.Chat;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces; // Added
using backend.Application.Knowledge.Commands.GenerateFamilyKb; // Added for GenerateFamilyKbCommand
using backend.Application.Knowledge.DTOs; // Added for KnowledgeSearchResultDto
using backend.Application.Knowledge.Queries.SearchKnowledgeBase; // Added for SearchKnowledgeBaseQuery
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    /// <summary>
    /// Sinh audio giọng nói từ text dựa trên giọng mẫu đã preprocess.
    /// </summary>
    /// <summary>
    /// Trigger việc tạo hoặc cập nhật Knowledge Base cho một Family.
    /// </summary>
    /// <param name="familyId">ID của Family cần tạo Knowledge Base.</param>
    /// <returns>Status 200 OK nếu quá trình được trigger thành công.</returns>
    [HttpPost("generate-kb/{familyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateFamilyKb(Guid familyId)
    {
        var command = new GenerateFamilyKbCommand { FamilyId = familyId };
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Tìm kiếm trong Knowledge Base.
    /// </summary>
    /// <param name="query">Yêu cầu tìm kiếm Knowledge Base.</param>
    /// <returns>Danh sách các kết quả tìm kiếm Knowledge Base.</returns>
    [HttpPost("search-kb")]
    [ProducesResponseType(typeof(List<KnowledgeSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchKnowledgeBase([FromBody] SearchKnowledgeBaseQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ToActionResult(this, _logger);
    }
}
