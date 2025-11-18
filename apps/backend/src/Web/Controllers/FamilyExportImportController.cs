using backend.Application.Common.Models;
using backend.Application.Families.ExportImport;
using backend.Application.Families.Queries.ExportImport;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/family-data")] // Changed route to avoid conflict with /api/family
public class FamilyExportImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public FamilyExportImportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{familyId}/export")]
    [ProducesResponseType(typeof(FamilyExportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FamilyExportDto>> ExportFamily(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyExportQuery(familyId));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return NotFound(result.Error);
    }

    [HttpPost("import/{familyId}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> ImportFamily(Guid familyId, [FromQuery] bool clearExistingData = true, [FromBody] FamilyExportDto familyData)
    {
        var command = new ImportFamilyCommand
        {
            FamilyId = familyId,
            FamilyData = familyData,
            ClearExistingData = clearExistingData
        };
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
