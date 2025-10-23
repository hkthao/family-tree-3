using backend.Application.Members.Commands.GenerateBiography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIController(IMediator mediator, ILogger<AIController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<AIController> _logger = logger; // Re-add this

    /// <summary>
    /// Generates a biography for a member using AI.
    /// </summary>
    /// <param name="command">The command to generate the biography.</param>
    /// <returns>The generated biography content and metadata.</returns>
    [HttpPost("biography")]
    public async Task<ActionResult<BiographyResultDto>> GenerateBiography([FromBody] GenerateBiographyCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? (ActionResult<BiographyResultDto>)Ok(result.Value) : (ActionResult<BiographyResultDto>)BadRequest(result.Error);
    }
}
