using backend.Application.AI.Commands.GenerateBiography;
using backend.Application.AI.Commands.SaveAIBiography; // Add this
using backend.Application.AI.Common;
using backend.Application.AI.Queries.GetLastAIBiography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AIController> _logger; // Re-add this

    public AIController(IMediator mediator, ILogger<AIController> logger) // Modify constructor
    {
        _mediator = mediator;
        _logger = logger; // Assign logger
    }

    /// <summary>
    /// Generates a biography for a member using AI.
    /// </summary>
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
    /// Retrieves the last AI-generated biography for a specific member.
    /// </summary>
    /// <param name="memberId">The ID of the member.</param>
    /// <returns>The last AI-generated biography.</returns>
    [HttpGet("biography/last/{memberId}")]
    public async Task<ActionResult<AIBiographyDto?>> GetLastAIBiography(Guid memberId)
    {
        var result = await _mediator.Send(new GetLastAIBiographyQuery { MemberId = memberId });
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }



    /// <summary>
    /// Saves an AI-generated biography for a member.
    /// </summary>
    /// <param name="command">The command to save the biography.</param>
    /// <returns>The ID of the saved biography.</returns>
    [HttpPost("biography/save")]
    public async Task<ActionResult<Guid>> SaveBiography([FromBody] SaveAIBiographyCommand command)
    {
        _logger.LogInformation("SaveBiography received command: {@Command}", command);

        if (command == null)
        {
            _logger.LogError("SaveBiography command is null.");
            return BadRequest("Command cannot be null.");
        }

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
