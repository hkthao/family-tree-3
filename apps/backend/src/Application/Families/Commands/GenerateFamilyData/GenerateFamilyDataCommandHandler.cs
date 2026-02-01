using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Services.LLMGateway; // NEW
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting; // NEW
using backend.Application.Common.Models.LLMGateway; // NEW
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Prompts.Queries.GetPromptById;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // NEW

namespace backend.Application.Families.Commands.GenerateFamilyData;

/// <summary>
/// Handler cho lệnh GenerateFamilyDataCommand.
/// </summary>
public class GenerateFamilyDataCommandHandler : IRequestHandler<GenerateFamilyDataCommand, Result<CombinedAiContentDto>>
{
    private readonly IMediator _mediator;
    private readonly ILLMGatewayService _llmGatewayService;
    private readonly ILogger<GenerateFamilyDataCommandHandler> _logger;
    private readonly LLMGatewaySettings _llmGatewaySettings; // NEW

    public GenerateFamilyDataCommandHandler(
        IMediator mediator,
        ILLMGatewayService llmGatewayService,
        ILogger<GenerateFamilyDataCommandHandler> logger,
        IOptions<LLMGatewaySettings> llmGatewaySettings) // NEW
    {
        _mediator = mediator;
        _llmGatewayService = llmGatewayService;
        _logger = logger;
        _llmGatewaySettings = llmGatewaySettings.Value; // NEW
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

        // 3. Chuẩn bị yêu cầu LLM Gateway
        var llmRequest = new LLMChatCompletionRequest
        {
            Model = _llmGatewaySettings.LlmModel,
            Messages = new List<LLMChatCompletionMessage>
            {
                new LLMChatCompletionMessage { Role = "system", Content = systemPromptContent },
                new LLMChatCompletionMessage { Role = "user", Content = request.ChatInput }
            },
            User = request.FamilyId.ToString(), // Use FamilyId as user identifier for LLM Gateway
            Metadata = new Dictionary<string, object>
            {
                { "familyId", request.FamilyId.ToString() },
                { "sessionId", Guid.NewGuid().ToString() } // New session ID for this request
            }
        };

        // 4. Gửi yêu cầu đến dịch vụ LLM Gateway và phân tích phản hồi
        var llmResponseResult = await _llmGatewayService.GetChatCompletionAsync(llmRequest, cancellationToken);

        if (!llmResponseResult.IsSuccess)
        {
            return Result<CombinedAiContentDto>.Failure(llmResponseResult.Error ?? "Lỗi không xác định khi gọi LLM Gateway.", llmResponseResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (llmResponseResult.Value == null || llmResponseResult.Value.Choices == null || !llmResponseResult.Value.Choices.Any())
        {
            return Result<CombinedAiContentDto>.Failure(ErrorMessages.NoAIResponse, ErrorSources.ExternalServiceError);
        }

        var combinedContent = llmResponseResult.Value.Choices.First().Message.Content;

        return Result<CombinedAiContentDto>.Success(new CombinedAiContentDto { CombinedContent = combinedContent });
    }

}
