using backend.Application.N8n.Commands.GenerateWebhookJwt;
using MediatR;
using Microsoft.AspNetCore.Authorization; // Added
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize] // Added
[ApiController] // Added
[Route("api/[controller]")]
public class N8nController : ControllerBase
{
    private readonly IMediator _mediator; // Added

    public N8nController(IMediator mediator) // Added constructor
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tạo một JWT token để gọi n8n webhook.
    /// </summary>
    /// <param name="command">Command chứa chủ đề (subject) và thời gian hết hạn của JWT.</param>
    /// <returns>JWT token đã tạo.</returns>
    [HttpPost("generate-webhook-jwt")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GenerateWebhookJwtResponse>> GenerateWebhookJwt([FromBody] GenerateWebhookJwtCommand command) // Added [FromBody]
    {
        var result = await _mediator.Send(command); // Changed to _mediator.Send
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result); // Changed to access result.IsSuccess and result.Value
    }
}
