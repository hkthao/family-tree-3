using Application.NaturalLanguageInput.Commands.GenerateData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NaturalLanguageInputController : ControllerBase
{
    private readonly IMediator _mediator;

    public NaturalLanguageInputController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate-data")]
    public async Task<ActionResult<string>> GenerateData([FromBody] GenerateDataCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}