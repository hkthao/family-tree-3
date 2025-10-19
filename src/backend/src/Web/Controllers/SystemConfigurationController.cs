using FamilyTree.Application.Common.Models;
using FamilyTree.Application.SystemConfigurations.Commands.CreateSystemConfiguration;
using FamilyTree.Application.SystemConfigurations.Commands.DeleteSystemConfiguration;
using FamilyTree.Application.SystemConfigurations.Commands.UpdateSystemConfiguration;
using FamilyTree.Application.SystemConfigurations.Queries.ListSystemConfigurations;
using FamilyTree.Application.SystemConfigurations.Queries.SystemConfigurationDto;
using Microsoft.AspNetCore.Mvc;

namespace FamilyTree.Web.Controllers;

public class SystemConfigurationController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Result<List<SystemConfigurationDto>>>> GetSystemConfigurations()
    {
        return await Mediator.Send(new ListSystemConfigurationsQuery());
    }

    [HttpPost]
    public async Task<ActionResult<Result<int>>> CreateSystemConfiguration(CreateSystemConfigurationCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Result>> UpdateSystemConfiguration(int id, UpdateSystemConfigurationCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        return await Mediator.Send(command);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> DeleteSystemConfiguration(int id)
    {
        return await Mediator.Send(new DeleteSystemConfigurationCommand(id));
    }
}
