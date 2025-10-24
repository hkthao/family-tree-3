using backend.Application.AI.Chat;
using backend.Application.AI.Chat.Queries;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

/// <summary>
/// Bộ điều khiển xử lý các yêu cầu liên quan đến trò chuyện với trợ lý AI.
/// </summary>
/// <param name="mediator">Đối tượng ISender để gửi các lệnh và truy vấn.</param>
[ApiController]
[Route("api/[controller]")]
public class ChatController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Đối tượng ISender để gửi các lệnh và truy vấn.
    /// </summary>
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Xử lý POST request để trò chuyện với trợ lý AI.
    /// </summary>
    /// <param name="request">Yêu cầu trò chuyện, bao gồm tin nhắn và ID phiên.</param>
    /// <returns>Phản hồi từ trợ lý AI.</returns>
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> ChatWithAssistant([FromBody] ChatRequest request)
    {
        var query = new ChatWithAssistantQuery(request.Message, request.SessionId);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? (ActionResult<ChatResponse>)Ok(result.Value) : (ActionResult<ChatResponse>)BadRequest(result.Error);
    }
}

/// <summary>
/// Đại diện cho yêu cầu trò chuyện với trợ lý AI.
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// Tin nhắn từ người dùng.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// ID phiên trò chuyện (tùy chọn).
    /// </summary>
    public string? SessionId { get; set; }
}
