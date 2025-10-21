using backend.Application.Common.Models;
using backend.Application.Common.Security;
using backend.Application.SystemConfigurations.Commands.CreateSystemConfiguration;
using backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;
using backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;
using backend.Application.SystemConfigurations.Queries;
using backend.Application.SystemConfigurations.Queries.ListSystemConfigurations;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SystemConfigurationController : ControllerBase
{
    private readonly IMediator _mediator;

    public SystemConfigurationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<List<SystemConfigurationDto>>>> GetSystemConfigurations()
    {
        return await _mediator.Send(new ListSystemConfigurationsQuery());
    }

    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> CreateSystemConfiguration([FromBody] CreateSystemConfigurationCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> UpdateSystemConfiguration([FromRoute] Guid id, [FromBody] UpdateSystemConfigurationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteSystemConfiguration([FromRoute] Guid id)
    {
        return await _mediator.Send(new DeleteSystemConfigurationCommand(id));
    }
}
