using Microsoft.AspNetCore.Mvc;
using backend.Application.Chat.Queries;
using backend.Application.Chat;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ISender _mediator;

    public ChatController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> ChatWithAssistant([FromBody] ChatRequest request)
    {
        var query = new ChatWithAssistantQuery(request.Message, request.SessionId);
        var response = await _mediator.Send(query);
        return Ok(response);
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}
