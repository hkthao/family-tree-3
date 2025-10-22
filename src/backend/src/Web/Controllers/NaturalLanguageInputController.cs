using Application.NaturalLanguageInput.Commands.GenerateData;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NaturalLanguageInputController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("generate-event-data")]
    public async Task<ActionResult<string>> GenerateEventData([FromBody] GenerateEventDataCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}
