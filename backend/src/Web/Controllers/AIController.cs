using backend.Application.AI.Commands.GenerateBiography;
using backend.Application.AI.Queries.GetLastUserPrompt;
using backend.Application.AI.Queries.GetAIProviders;
using backend.Application.AI.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Application.AI.Queries;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;

    public AIController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generates a biography for a member using AI.
    /// </summary>
    /// <param name="memberId">The ID of the member.</param>
    /// <param name="command">The command to generate the biography.</param>
    /// <returns>The generated biography content and metadata.</returns>
    [HttpPost("biography")]
    public async Task<ActionResult<BiographyResultDto>> GenerateBiography([FromBody] GenerateBiographyCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves the last user prompt used for a specific member.
    /// </summary>
    /// <param name="memberId">The ID of the member.</param>
    /// <returns>The last user prompt string.</returns>
    [HttpGet("biography/last-prompt/{memberId}")]
    public async Task<ActionResult<string?>> GetLastUserPrompt(Guid memberId)
    {
        var result = await _mediator.Send(new GetLastUserPromptQuery { MemberId = memberId });
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves a list of available AI providers and their current usage status.
    /// </summary>
    /// <returns>A list of AI providers with their status and usage.</returns>
    [HttpGet("biography/providers")]
    public async Task<ActionResult<List<AIProviderDto>>> GetAIProviders()
    {
        var result = await _mediator.Send(new GetAIProvidersQuery());
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
