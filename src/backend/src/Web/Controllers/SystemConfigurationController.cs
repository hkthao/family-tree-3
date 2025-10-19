using backend.Application.Common.Models;
using backend.Application.SystemConfigurations.Commands.CreateSystemConfiguration;
using backend.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;
using backend.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;
using backend.Application.SystemConfigurations.Queries.ListSystemConfigurations;
using backend.Application.SystemConfigurations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

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
    public async Task<ActionResult<Result<Guid>>> CreateSystemConfiguration(CreateSystemConfigurationCommand command)
    {
        return await _mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> UpdateSystemConfiguration(Guid id, UpdateSystemConfigurationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        return await _mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteSystemConfiguration(Guid id)
    {
        return await _mediator.Send(new DeleteSystemConfigurationCommand(id));
    }
}
