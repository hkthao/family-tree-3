using System.Text.Json; // NEW
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants; // Add this for PromptConstants
using backend.Application.Common.Interfaces.Services.LLMGateway; // NEW
using backend.Application.Common.Models;
using backend.Application.Common.Models.AppSetting;
using backend.Application.Common.Models.LLMGateway; // NEW
using backend.Application.Prompts.Queries.GetPromptById; // Add this for GetPromptByIdQuery
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace backend.Application.AI.Commands.DetermineChatContext;

/// <summary>
/// Xử lý lệnh để xác định ngữ cảnh của một tin nhắn chat từ người dùng.
/// </summary>
public class DetermineChatContextCommandHandler : IRequestHandler<DetermineChatContextCommand, Result<ContextClassificationDto>>
{
    private readonly ILLMGatewayService _llmGatewayService; // Change to LLM Gateway Service
    private readonly ILogger<DetermineChatContextCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly LLMGatewaySettings _llmGatewaySettings; // Changed from ChatSettings

    public DetermineChatContextCommandHandler(
        ILLMGatewayService llmGatewayService, // Change injection
        ILogger<DetermineChatContextCommandHandler> logger,
        IMediator mediator,
        IOptions<LLMGatewaySettings> llmGatewaySettings) // Inject IOptions for LLMGatewaySettings
    {
        _llmGatewayService = llmGatewayService;
        _logger = logger;
        _mediator = mediator;
        _llmGatewaySettings = llmGatewaySettings.Value; // Extract LLMGatewaySettings
    }

    public async Task<Result<ContextClassificationDto>> Handle(DetermineChatContextCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang xác định ngữ cảnh cho tin nhắn: {ChatMessage}", request.ChatMessage);

        // 1. Lấy System Prompt thống nhất
        var promptResult = await _mediator.Send(new GetPromptByIdQuery { Code = PromptConstants.CONTEXT_CLASSIFICATION_PROMPT }, cancellationToken);

        if (!promptResult.IsSuccess)
        {
            _logger.LogError("Không thể lấy system prompt '{PromptCode}' từ cơ sở dữ liệu. Hủy phân tích ngữ cảnh. Lỗi: {Error}", PromptConstants.CONTEXT_CLASSIFICATION_PROMPT, promptResult.Error);
            return Result<ContextClassificationDto>.Failure($"System prompt for '{PromptConstants.CONTEXT_CLASSIFICATION_PROMPT}' not configured or fetched from the database.", promptResult.ErrorSource ?? "Unknown");
        }

        var systemPrompt = promptResult.Value?.Content;

        if (string.IsNullOrEmpty(systemPrompt))
        {
            _logger.LogError("Nội dung system prompt cho '{PromptCode}' lấy từ database là null hoặc trống. Hủy tạo AI.", PromptConstants.CONTEXT_CLASSIFICATION_PROMPT);
            return Result<ContextClassificationDto>.Failure($"Nội dung system prompt cho '{PromptConstants.CONTEXT_CLASSIFICATION_PROMPT}' là null hoặc trống.", "Configuration");
        }

        var llmChatRequest = new LLMChatCompletionRequest
        {
            Model = _llmGatewaySettings.LlmModel, // Use configured LLM Model
            Messages = new List<LLMChatCompletionMessage>
            {
                new LLMChatCompletionMessage { Role = "system", Content = systemPrompt },
                new LLMChatCompletionMessage { Role = "user", Content = request.ChatMessage }
            },
            Temperature = 0.0f, // Use a low temperature for classification tasks
            MaxTokens = 500 // Limit output size for classification
        };

        var llmResult = await _llmGatewayService.GetChatCompletionAsync(llmChatRequest, cancellationToken); // Call LLM Gateway

        if (!llmResult.IsSuccess)
        {
            _logger.LogError("Phân tích ngữ cảnh AI thất bại từ LLM Gateway: {Error}", llmResult.Error);
            return Result<ContextClassificationDto>.Failure(llmResult.Error ?? "Không thể phân tích ngữ cảnh.", llmResult.ErrorSource ?? "ExternalServiceError");
        }

        if (llmResult.Value == null || !llmResult.Value.Choices.Any() || string.IsNullOrWhiteSpace(llmResult.Value.Choices[0].Message.Content))
        {
            _logger.LogWarning("LLM Gateway trả về kết quả rỗng hoặc không có nội dung khi phân tích ngữ cảnh cho tin nhắn: {ChatMessage}", request.ChatMessage);
            return Result<ContextClassificationDto>.Failure("LLM Gateway trả về kết quả rỗng khi phân tích ngữ cảnh.", "AIError");
        }

        // Parse the LLM's output into ContextClassificationDto
        try
        {
            var contextClassification = JsonSerializer.Deserialize<ContextClassificationDto>(llmResult.Value.Choices[0].Message.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (contextClassification == null)
            {
                _logger.LogWarning("Không thể phân tích JSON từ phản hồi của LLM Gateway vào ContextClassificationDto. Phản hồi: {Content}", llmResult.Value.Choices[0].Message.Content);
                return Result<ContextClassificationDto>.Failure("Không thể phân tích phản hồi của AI để xác định ngữ cảnh.", "AIError");
            }

            _logger.LogInformation("Ngữ cảnh được xác định: {ContextType} với lý do: {Reasoning}", contextClassification.Context, contextClassification.Reasoning);
            return Result<ContextClassificationDto>.Success(contextClassification);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Lỗi phân tích JSON từ phản hồi của LLM Gateway: {Content}", llmResult.Value.Choices[0].Message.Content);
            return Result<ContextClassificationDto>.Failure($"Lỗi phân tích phản hồi của AI: {ex.Message}", "AIError");
        }
    }
}
