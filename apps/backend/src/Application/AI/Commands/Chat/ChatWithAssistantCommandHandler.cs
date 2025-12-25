using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;
using backend.Application.AI.Commands.DetermineChatContext;
using backend.Application.AI.Enums;
using backend.Application.Families.Commands.GenerateFamilyData;
using Microsoft.Extensions.Options; // Add this
using backend.Application.Common.Models.AppSetting; // Add this
using backend.Application.Prompts.Queries.GetPromptById;
using backend.Application.AI.Commands.Chat.CallAiChatService;
using backend.Application.Families.Commands.EnsureFamilyAiConfigExists;
using backend.Application.Families.Queries.CheckAiChatQuota;
using backend.Application.Common.Queries.ValidateUserAuthentication;
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;

namespace backend.Application.AI.Chat;

/// <summary>
/// Handler cho lệnh <see cref="ChatWithAssistantCommand"/>.
/// Đây là một Orchestrator, điều phối các hoạt động AI chat.
/// </summary>
public class ChatWithAssistantCommandHandler : IRequestHandler<ChatWithAssistantCommand, Result<ChatResponse>>
{
    private readonly ILogger<ChatWithAssistantCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly N8nSettings _n8nSettings;

    public ChatWithAssistantCommandHandler(
        ILogger<ChatWithAssistantCommandHandler> logger,
        IMediator mediator,
        IOptions<N8nSettings> n8nSettings)
    {
        _logger = logger;
        _mediator = mediator;
        _n8nSettings = n8nSettings.Value;
    }


    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bắt đầu xử lý yêu cầu trợ lý chat cho FamilyId: {FamilyId}", request.FamilyId);

        // 1. Kiểm tra xác thực người dùng và quyền truy cập
        var authResult = await _mediator.Send(new ValidateUserAuthenticationQuery(), cancellationToken);
        if (!authResult.IsSuccess)
        {
            string error = authResult.Error ?? ErrorMessages.Unauthorized;
            string errorSource = authResult.ErrorSource ?? ErrorSources.Authentication;
            return Result<ChatResponse>.Failure(error, errorSource);
        }

        // 2. Đảm bảo cấu hình giới hạn AI cho gia đình tồn tại
        var ensureConfigResult = await _mediator.Send(new EnsureFamilyAiConfigExistsCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!ensureConfigResult.IsSuccess)
        {
            string error = ensureConfigResult.Error ?? "Không thể đảm bảo cấu hình giới hạn AI của gia đình.";
            string errorSource = ensureConfigResult.ErrorSource ?? "Unknown";
            return Result<ChatResponse>.Failure(error, errorSource);
        }

        // 3. Kiểm tra hạn ngạch AI Chat (áp dụng cho cả chat và data generation)
        var quotaCheckResult = await _mediator.Send(new CheckAiChatQuotaQuery { FamilyId = request.FamilyId }, cancellationToken);
        if (!quotaCheckResult.IsSuccess)
        {
            string error = quotaCheckResult.Error ?? ErrorMessages.AiChatQuotaExceeded;
            string errorSource = quotaCheckResult.ErrorSource ?? ErrorSources.QuotaExceeded;
            return Result<ChatResponse>.Failure(error, errorSource);
        }

        // 4. Xác định ngữ cảnh tin nhắn
        _logger.LogInformation("Đang xác định ngữ cảnh cho tin nhắn chat.");
        var determineContextCommand = new DetermineChatContextCommand
        {
            ChatMessage = request.ChatInput,
            SessionId = request.SessionId
        };
        var contextResult = await _mediator.Send(determineContextCommand, cancellationToken);

        ContextType determinedContext = ContextType.Unknown;
        string contextReasoning = "Not provided.";

        if (contextResult.IsSuccess && contextResult.Value != null)
        {
            determinedContext = contextResult.Value.Context;
            contextReasoning = contextResult.Value.Reasoning ?? contextReasoning;
            _logger.LogInformation("Ngữ cảnh được xác định: {ContextType} với lý do: {Reasoning}", determinedContext, contextReasoning);
        }
        else
        {
            _logger.LogError("Không thể xác định ngữ cảnh chat: {Error}. Fallback về ngữ cảnh QA.", contextResult.Error);
            determinedContext = ContextType.QA; // Fallback to QA
            contextReasoning = "Fallback due to context determination failure or explicit Unknown.";
        }

        // 5. Lấy System Prompt chung cho QA (dùng cho các ngữ cảnh chat thông thường)
        string qaSystemPromptContent = "You are a helpful assistant."; // Default fallback prompt
        var qaPromptResult = await _mediator.Send(new GetPromptByIdQuery { Code = PromptConstants.CHAT_QA_PROMPT }, cancellationToken);
        if (qaPromptResult.IsSuccess && qaPromptResult.Value?.Content != null)
        {
            qaSystemPromptContent = qaPromptResult.Value.Content;
        }
        else
        {
            _logger.LogError("Không thể lấy system prompt '{PromptCode}' từ database. Sử dụng QA mặc định.", PromptConstants.CHAT_QA_PROMPT);
        }

        // 6. Xử lý dựa trên ngữ cảnh
        Result<ChatResponse> finalChatResponseResult;
        switch (determinedContext)
        {
            case ContextType.QA:
            case ContextType.FamilyDataLookup:
            case ContextType.Unknown:
                _logger.LogInformation("Ngữ cảnh là QA/FamilyDataLookup/Unknown. Gọi dịch vụ AI Chat truyền thống.");
                var callChatServiceCommand = new CallAiChatServiceCommand
                {
                    ChatRequest = new ChatRequest
                    {
                        SessionId = request.SessionId,
                        ChatInput = request.ChatInput,
                        Metadata = request.Metadata ?? new Dictionary<string, object>(),
                        Context = determinedContext,
                        SystemPrompt = qaSystemPromptContent,
                        CollectionName = _n8nSettings.Chat.CollectionName
                    }
                };
                callChatServiceCommand.ChatRequest.Metadata[MetadataConstants.FamilyId] = request.FamilyId.ToString();
                finalChatResponseResult = await _mediator.Send(callChatServiceCommand, cancellationToken);
                break;

            case ContextType.DataGeneration:
                _logger.LogInformation("Ngữ cảnh là DataGeneration. Gọi lệnh tạo dữ liệu gia đình.");
                var generateDataCommand = new GenerateFamilyDataCommand
                {
                    FamilyId = request.FamilyId,
                    ChatInput = request.ChatInput
                };
                var generatedDataResult = await _mediator.Send(generateDataCommand, cancellationToken);

                if (!generatedDataResult.IsSuccess)
                {
                    string error = generatedDataResult.Error ?? "Không thể tạo dữ liệu gia đình.";
                    string errorSource = generatedDataResult.ErrorSource ?? "Unknown";
                    finalChatResponseResult = Result<ChatResponse>.Failure(error, errorSource);
                }
                else
                {
                    finalChatResponseResult = generatedDataResult.Value != null
                        ? Result<ChatResponse>.Success(new ChatResponse
                        {
                            Output = "Dữ liệu gia đình đã được tạo thành công.",
                            GeneratedData = generatedDataResult.Value
                        })
                        : Result<ChatResponse>.Failure("Lỗi không xác định khi tạo dữ liệu gia đình: Kết quả rỗng.", "InternalError");
                }
                break;

            default:
                _logger.LogWarning("Ngữ cảnh không được xử lý: {Context}. Mặc định về QA.", determinedContext);
                goto case ContextType.QA;
        }

        // 7. Ghi nhận sử dụng AI Chat
        var incrementUsageResult = await _mediator.Send(new IncrementFamilyAiChatUsageCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!incrementUsageResult.IsSuccess)
        {
            _logger.LogError("Không thể tăng hạn ngạch AI Chat cho FamilyId {FamilyId}: {Error}", request.FamilyId, incrementUsageResult.Error);
            // Vẫn trả về kết quả chính, nhưng log lỗi tăng hạn ngạch
        }

        _logger.LogInformation("AI Chat usage incremented for FamilyId {FamilyId}. Final result: {IsSuccess}",
            request.FamilyId, finalChatResponseResult.IsSuccess);

        return finalChatResponseResult;
    }
}
