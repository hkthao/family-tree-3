using backend.Application.Members.Commands.GenerateBiography;
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
}
