using backend.Application.Common.Models;
using backend.Application.PrivacyConfigurations.Commands;
using backend.Application.PrivacyConfigurations.Queries;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

public class PrivacyConfigurationController : ApiControllerBase
{
    [HttpGet("{familyId}")]
    public async Task<ActionResult<Result<PrivacyConfigurationDto>>> GetPrivacyConfiguration(Guid familyId)
    {
        return await Mediator.Send(new GetPrivacyConfigurationQuery(familyId));
    }

    [HttpPut("{familyId}")]
    public async Task<ActionResult<Result<Unit>>> UpdatePrivacyConfiguration(Guid familyId, UpdatePrivacyConfigurationCommand command)
    {
        if (familyId != command.FamilyId)
        {
            return BadRequest();
        }
        return await Mediator.Send(command);
    }
}
