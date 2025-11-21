using backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;
using backend.Application.NaturalLanguage.Models; // Add using directive for AnalyzedDataDto
using Microsoft.AspNetCore.Mvc;

namespace backend.Web.Controllers;

[ApiController] // Add ApiController attribute
[Route("api/natural-language")] // Add base route for the controller
public class NaturalLanguageController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Phân tích văn bản ngôn ngữ tự nhiên và tạo prompt cho AI Agent.
    /// </summary>
    /// <param name="command">Lệnh chứa nội dung văn bản cần phân tích.</param>
    /// <returns>Kết quả từ AI Agent.</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<AnalyzedDataDto>> Analyze([FromBody] AnalyzeNaturalLanguageCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(result.Error);
    }
}
