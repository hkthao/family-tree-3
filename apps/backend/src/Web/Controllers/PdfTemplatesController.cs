using backend.Application.PdfTemplates.Commands.CreatePdfTemplate;
using backend.Application.PdfTemplates.Commands.DeletePdfTemplate;
using backend.Application.PdfTemplates.Commands.UpdatePdfTemplate;
using backend.Application.PdfTemplates.Dtos;
using backend.Application.PdfTemplates.Queries.GetPdfTemplate;
using backend.Application.PdfTemplates.Queries.GetPdfTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/pdf-templates")]
public class PdfTemplatesController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Lấy một template PDF theo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PdfTemplateDto>> GetPdfTemplateById(Guid id, [FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new GetPdfTemplateQuery(id, familyId));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.NotFound)
        {
            return NotFound(result.Error);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy danh sách các template PDF cho một Family.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PdfTemplateDto>>> GetPdfTemplates([FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new GetPdfTemplatesQuery(familyId));
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Tạo một template PDF mới.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PdfTemplateDto>> CreatePdfTemplate([FromBody] CreatePdfTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            // Add null check for result.Value
            if (result.Value == null)
            {
                return BadRequest("Failed to create template: Result value is null.");
            }
            return CreatedAtAction(nameof(GetPdfTemplateById), new { id = result.Value.Id, familyId = result.Value.FamilyId }, result.Value);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Cập nhật một template PDF hiện có.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Unit>> UpdatePdfTemplate(Guid id, [FromBody] UpdatePdfTemplateCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Template ID in URL does not match body.");
        }
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return NoContent();
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.NotFound)
        {
            return NotFound(result.Error);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return BadRequest(result.Error);
    }

    /// <summary>
    /// Xóa một template PDF.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<Unit>> DeletePdfTemplate(Guid id, [FromQuery] Guid familyId)
    {
        var result = await _mediator.Send(new DeletePdfTemplateCommand(id, familyId));
        if (result.IsSuccess)
        {
            return NoContent();
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.NotFound)
        {
            return NotFound(result.Error);
        }
        if (result.ErrorSource == backend.Application.Common.Constants.ErrorSources.Forbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, result.Error);
        }
        return BadRequest(result.Error);
    }
}
