using MediatR;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using System.Text.RegularExpressions;
using backend.Application.Common.Exceptions;
using backend.Application.AI.Prompts;
using backend.Application.NaturalLanguage.Models; // Add using directive for AnalyzedDataDto
using System.Text.Json; // Add using directive for JsonSerializer

namespace backend.Application.NaturalLanguage.Commands.AnalyzeNaturalLanguage;

/// <summary>
/// Xử lý lệnh phân tích văn bản ngôn ngữ tự nhiên.
/// </summary>
public class AnalyzeNaturalLanguageCommandHandler : IRequestHandler<AnalyzeNaturalLanguageCommand, Result<AnalyzedDataDto>>
{
    private readonly IN8nService _n8nService;
    private readonly ICurrentUser _currentUserService;

    public AnalyzeNaturalLanguageCommandHandler(IN8nService n8nService, ICurrentUser currentUserService)
    {
        _n8nService = n8nService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AnalyzedDataDto>> Handle(AnalyzeNaturalLanguageCommand request, CancellationToken cancellationToken)
    {
        var sessionId = request.SessionId;

        // 1. Xây dựng prompt cho AI Agent (bao gồm phân tích mention bên trong PromptBuilder)
        var aiPrompt = PromptBuilder.BuildNaturalLanguageAnalysisPrompt(request.Content);

        // 2. Gửi prompt đến n8n Service chat
        var n8nResult = await _n8nService.CallChatWebhookAsync(sessionId, aiPrompt, cancellationToken);

        if (!n8nResult.IsSuccess)
        {
            throw new Common.Exceptions.ValidationException(n8nResult.Error ?? "Lỗi không xác định từ N8nService.");
        }

        // 3. Phân tích phản hồi từ AI (dự kiến là JSON)
        if (string.IsNullOrWhiteSpace(n8nResult.Value))
        {
            return Result<AnalyzedDataDto>.Failure("Phản hồi từ AI trống hoặc không hợp lệ.", "AIResponse");
        }

        try
        {
            var analyzedData = JsonSerializer.Deserialize<AnalyzedDataDto>(n8nResult.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (analyzedData == null)
            {
                return Result<AnalyzedDataDto>.Failure("Phản hồi từ AI không hợp lệ hoặc trống.", "AIResponse");
            }

            return Result<AnalyzedDataDto>.Success(analyzedData);
        }
        catch (JsonException ex)
        {
            return Result<AnalyzedDataDto>.Failure($"Không thể phân tích phản hồi JSON từ AI: {ex.Message}", "AIResponseParsing");
        }
        catch (Exception ex)
        {
            return Result<AnalyzedDataDto>.Failure($"Đã xảy ra lỗi khi xử lý phản hồi từ AI: {ex.Message}", "AIResponseProcessing");
        }
    }
}
