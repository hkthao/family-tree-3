using backend.Application.Families.ExportImport; // Added back for FamilyExportDto and ImportFamilyCommand
using backend.Application.Families.Queries.ExportImport;
using backend.Application.Families.Queries.ExportPdf; // Added for PDF export
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
    public async Task<ActionResult<Guid>> ImportFamily(Guid familyId, [FromBody] FamilyExportDto familyData, [FromQuery] bool clearExistingData = true)
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

    [HttpPost("{familyId}/export-pdf")] // Changed to HttpPost to receive HTML content
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ExportFamilyPdf(Guid familyId, [FromBody] string htmlContent)
    {
        var result = await _mediator.Send(new GetFamilyPdfExportQuery(familyId, htmlContent));
        if (result.IsSuccess)
        {
            return File(result.Value!.Content, "application/pdf", result.Value.FileName); // Changed from Stream to Content
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.NotFound)
        {
            return NotFound(result.Error);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return BadRequest(result.Error); // For any other errors
    }
}
