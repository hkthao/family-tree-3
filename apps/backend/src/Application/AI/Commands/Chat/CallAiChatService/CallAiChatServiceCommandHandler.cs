using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces.Services.LLMGateway; // NEW
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Commands.Chat.CallAiChatService;

/// <summary>
/// Xử lý lệnh để gọi dịch vụ AI Chat bên ngoài.
/// </summary>
public class CallAiChatServiceCommandHandler : IRequestHandler<CallAiChatServiceCommand, Result<ChatResponse>>
{
    private readonly ILLMGatewayService _llmGatewayService; // Change to LLM Gateway Service
    private readonly ILogger<CallAiChatServiceCommandHandler> _logger;

    public CallAiChatServiceCommandHandler(
        ILLMGatewayService llmGatewayService, // Change injection
        ILogger<CallAiChatServiceCommandHandler> logger)
    {
        _llmGatewayService = llmGatewayService;
        _logger = logger;
    }

    public async Task<Result<ChatResponse>> Handle(CallAiChatServiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang gọi dịch vụ LLM Gateway Chat với tin nhắn đầu tiên: {FirstMessage}", request.LLMChatCompletionRequest.Messages.FirstOrDefault()?.Content ?? "N/A"); // Adjust logging

        // Gọi dịch vụ LLM Gateway Chat
        var llmChatResponseResult = await _llmGatewayService.GetChatCompletionAsync(request.LLMChatCompletionRequest, cancellationToken); // Call LLM Gateway

        if (!llmChatResponseResult.IsSuccess)
        {
            _logger.LogError("Gọi dịch vụ LLM Gateway Chat thất bại: {Error}", llmChatResponseResult.Error);
            return Result<ChatResponse>.Failure(llmChatResponseResult.Error ?? "Không thể nhận phản hồi từ dịch vụ LLM Gateway Chat.", llmChatResponseResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (llmChatResponseResult.Value == null || !llmChatResponseResult.Value.Choices.Any())
        {
            return Result<ChatResponse>.Failure("LLM Gateway Chat trả về phản hồi rỗng hoặc không có lựa chọn nào.", ErrorSources.ExternalServiceError);
        }

        // Map LLMChatCompletionResponse to ChatResponse
        var chatResponse = new ChatResponse
        {
            Output = llmChatResponseResult.Value.Choices[0].Message.Content,
            // Các trường khác của ChatResponse (như GeneratedData, Intent, FaceDetectionResults)
            // sẽ không được điền từ phản hồi của LLM Gateway Chat cơ bản này.
            // Chúng sẽ được xử lý ở tầng cao hơn (ví dụ: ChatWithAssistantCommandHandler) nếu cần.
        };

        _logger.LogInformation("Đã nhận phản hồi từ dịch vụ LLM Gateway Chat.");
        return Result<ChatResponse>.Success(chatResponse);
    }
}
