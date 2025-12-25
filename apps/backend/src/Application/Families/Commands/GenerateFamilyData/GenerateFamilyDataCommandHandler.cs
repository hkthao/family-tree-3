using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Prompts.Queries.GetPromptById;
using Microsoft.Extensions.Logging;

namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Handler cho lệnh GenerateFamilyDataCommand.
/// </summary>
public class GenerateFamilyDataCommandHandler : IRequestHandler<GenerateFamilyDataCommand, Result<CombinedAiContentDto>>
{
    private readonly IMediator _mediator;
    private readonly IAiGenerateService _aiGenerateService;
    private readonly ILogger<GenerateFamilyDataCommandHandler> _logger;

    public GenerateFamilyDataCommandHandler(IMediator mediator, IAiGenerateService aiGenerateService, ILogger<GenerateFamilyDataCommandHandler> logger)
    {
        _mediator = mediator;
        _aiGenerateService = aiGenerateService;
        _logger = logger;
    }

    public async Task<Result<CombinedAiContentDto>> Handle(GenerateFamilyDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra và tăng hạn mức sử dụng AI
        var incrementUsageResult = await _mediator.Send(new IncrementFamilyAiChatUsageCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!incrementUsageResult.IsSuccess)
        {
            _logger.LogWarning("AI content generation failed for family {FamilyId} due to quota: {Error}", request.FamilyId, incrementUsageResult.Error);
            return Result<CombinedAiContentDto>.Failure(incrementUsageResult.Error ?? ErrorMessages.AiChatQuotaExceeded, incrementUsageResult.ErrorSource ?? ErrorSources.QuotaExceeded);
        }

        // 2. Lấy System Prompt thống nhất
        string promptCode = PromptConstants.FAMILY_DATA_GENERATION_PROMPT;
        var promptResult = await _mediator.Send(new GetPromptByIdQuery { Code = promptCode }, cancellationToken);

        string systemPromptContent;
        if (promptResult.IsSuccess && promptResult.Value != null)
        {
            systemPromptContent = promptResult.Value.Content;
            _logger.LogInformation("Successfully fetched unified system prompt '{PromptCode}' from database.", promptCode);
        }
        else
        {
            var errorMessage = $"System prompt for '{promptCode}' not configured in the database.";
            _logger.LogError(errorMessage);
            return Result<CombinedAiContentDto>.Failure(errorMessage, ErrorSources.InvalidConfiguration);
        }

        // 3. Chuẩn bị yêu cầu AI
        var aiRequest = new GenerateRequest
        {
            SystemPrompt = systemPromptContent,
            ChatInput = request.ChatInput,
            SessionId = Guid.NewGuid().ToString(), // Tạo SessionId mới cho mỗi yêu cầu
            Metadata = new Dictionary<string, object>
            {
                { "familyId", request.FamilyId }
            }
        };

        // 4. Gửi yêu cầu đến dịch vụ AI và phân tích phản hồi tổng hợp
        var combinedResult = await _aiGenerateService.GenerateDataAsync<CombinedAiContentDto>(aiRequest, cancellationToken);

        if (!combinedResult.IsSuccess)
        {
            return Result<CombinedAiContentDto>.Failure(combinedResult.Error ?? "Lỗi không xác định khi tạo nội dung AI tổng hợp.", combinedResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (combinedResult.Value == null)
        {
            return Result<CombinedAiContentDto>.Failure(ErrorMessages.NoAIResponse, ErrorSources.ExternalServiceError);
        }

        return Result<CombinedAiContentDto>.Success(combinedResult.Value);
    }

}
