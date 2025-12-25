using backend.Application.AI.Commands.DetermineChatContext;
using MediatR;
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants; // Add this for PromptConstants
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Prompts.Queries.GetPromptById; // Add this for GetPromptByIdQuery
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Commands.DetermineChatContext;

/// <summary>
/// Xử lý lệnh để xác định ngữ cảnh của một tin nhắn chat từ người dùng.
/// </summary>
public class DetermineChatContextCommandHandler : IRequestHandler<DetermineChatContextCommand, Result<ContextClassificationDto>>
{
    private readonly IAiGenerateService _aiGenerateService;
    private readonly ILogger<DetermineChatContextCommandHandler> _logger;
    private readonly IMediator _mediator; // Add IMediator

    public DetermineChatContextCommandHandler(
        IAiGenerateService aiGenerateService,
        ILogger<DetermineChatContextCommandHandler> logger,
        IMediator mediator) // Inject IMediator
    {
        _aiGenerateService = aiGenerateService;
        _logger = logger;
        _mediator = mediator; // Initialize IMediator
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
        
        // Use the fetched prompt's Value, ensuring it's not null before assignment
        var systemPrompt = promptResult.Value?.Content ?? throw new InvalidOperationException("System prompt content fetched from database is null or empty.");

        var generateRequest = new GenerateRequest
        {
            SessionId = request.SessionId,
            ChatInput = request.ChatMessage,
            SystemPrompt = systemPrompt // Use the fetched prompt
        };

        var aiResult = await _aiGenerateService.GenerateDataAsync<ContextClassificationDto>(generateRequest, cancellationToken);

        if (!aiResult.IsSuccess)
        {
            _logger.LogError("Phân tích ngữ cảnh AI thất bại: {Error}", aiResult.Error);
            return Result<ContextClassificationDto>.Failure(aiResult.Error ?? "Không thể phân tích ngữ cảnh.", aiResult.ErrorSource ?? "Unknown");
        }

        if (aiResult.Value == null)
        {
            _logger.LogWarning("AI trả về kết quả rỗng khi phân tích ngữ cảnh cho tin nhắn: {ChatMessage}", request.ChatMessage);
            return Result<ContextClassificationDto>.Failure("AI trả về kết quả rỗng khi phân tích ngữ cảnh.", "AIError");
        }

        _logger.LogInformation("Ngữ cảnh được xác định: {ContextType} với lý do: {Reasoning}", aiResult.Value.Context, aiResult.Value.Reasoning);
        return Result<ContextClassificationDto>.Success(aiResult.Value);
    }
}
