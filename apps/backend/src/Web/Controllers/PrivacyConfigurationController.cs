using backend.Application.Common.Models;
using backend.Application.PrivacyConfigurations.Commands;
using backend.Application.PrivacyConfigurations.Queries;
using MediatR; // Add this using directive
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/PrivacyConfiguration")]
public class PrivacyConfigurationController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("{familyId}")]
    public async Task<ActionResult<Result<PrivacyConfigurationDto>>> GetPrivacyConfiguration(Guid familyId)
    {
        return await _mediator.Send(new GetPrivacyConfigurationQuery(familyId));
    }

    [HttpPut("{familyId}")]
    public async Task<ActionResult<Result<Unit>>> UpdatePrivacyConfiguration(Guid familyId, UpdatePrivacyConfigurationCommand command)
    {
        if (familyId != command.FamilyId)
        {
            return BadRequest();
        }
        return await _mediator.Send(command);
    }
}
