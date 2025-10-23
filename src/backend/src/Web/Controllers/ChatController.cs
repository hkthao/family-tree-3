using backend.Application.AI.Chat;
using backend.Application.AI.Chat.Queries;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> ChatWithAssistant([FromBody] ChatRequest request)
    {
        var query = new ChatWithAssistantQuery(request.Message, request.SessionId);
        var result = await _mediator.Send(query);

        return result.IsSuccess ? (ActionResult<ChatResponse>)Ok(result.Value) : (ActionResult<ChatResponse>)BadRequest(result.Error);
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}
