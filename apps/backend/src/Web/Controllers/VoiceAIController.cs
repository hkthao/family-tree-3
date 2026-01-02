using Microsoft.AspNetCore.Mvc;
using backend.Application.Common.Interfaces;
using backend.Application.Voice.DTOs;
using backend.Application.Common.Models; // Added for ToActionResult extension method
using Microsoft.Extensions.Logging; // Added for ILogger

namespace backend.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoiceAIController : ControllerBase
{
    private readonly IVoiceAIService _voiceAIService;
    private readonly ILogger<VoiceAIController> _logger; // Added logger

    public VoiceAIController(IVoiceAIService voiceAIService, ILogger<VoiceAIController> logger)
    {
        _voiceAIService = voiceAIService;
        _logger = logger; // Store logger
    }

    /// <summary>
    /// Tiền xử lý nhiều đoạn audio giọng nói thành 1 file WAV sạch.
    /// </summary>
    /// <param name="request">Yêu cầu tiền xử lý giọng nói.</param>
    /// <returns>URL của file audio đã xử lý và tổng thời lượng.</returns>
    [HttpPost("preprocess")]
    [ProducesResponseType(typeof(VoicePreprocessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Preprocess([FromBody] VoicePreprocessRequest request)
    {
        var result = await _voiceAIService.PreprocessVoiceAsync(request);
        return result.ToActionResult(this, _logger);
    }

    /// <summary>
    /// Sinh audio giọng nói từ text dựa trên giọng mẫu đã preprocess.
    /// </summary>
    /// <param name="request">Yêu cầu sinh giọng nói.</param>
    /// <returns>URL của file audio đã sinh ra.</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(VoiceGenerateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate([FromBody] VoiceGenerateRequest request)
    {
        var result = await _voiceAIService.GenerateVoiceAsync(request);
        return result.ToActionResult(this, _logger);
    }
}
