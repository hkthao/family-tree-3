using backend.Application.ExportImport.Commands;
using backend.Application.ExportImport.Queries;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController]
[Route("api/family-data")] // Changed route to avoid conflict with /api/family
public class FamilyExportImportController(IMediator mediator, ILogger<FamilyExportImportController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<FamilyExportImportController> _logger = logger;

    [HttpGet("{familyId}/export")]
    [ProducesResponseType(typeof(FamilyExportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportFamily(Guid familyId)
    {
        var result = await _mediator.Send(new GetFamilyExportQuery(familyId));
        return result.ToActionResult(this, _logger);
    }

    [HttpPost("import/{familyId}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImportFamily(Guid familyId, [FromBody] FamilyExportDto familyData, [FromQuery] bool clearExistingData = true)
    {
        var command = new ImportFamilyCommand
        {
            FamilyId = familyId,
            FamilyData = familyData,
            ClearExistingData = clearExistingData
        };
        var result = await _mediator.Send(command);
        return result.ToActionResult(this, _logger);
    }
}
