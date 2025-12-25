using MediatR;
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Commands.Chat.CallAiChatService;

/// <summary>
/// Xử lý lệnh để gọi dịch vụ AI Chat bên ngoài.
/// </summary>
public class CallAiChatServiceCommandHandler : IRequestHandler<CallAiChatServiceCommand, Result<ChatResponse>>
{
    private readonly IAiChatService _aiChatService;
    private readonly ILogger<CallAiChatServiceCommandHandler> _logger;

    public CallAiChatServiceCommandHandler(IAiChatService aiChatService, ILogger<CallAiChatServiceCommandHandler> logger)
    {
        _aiChatService = aiChatService;
        _logger = logger;
    }

    public async Task<Result<ChatResponse>> Handle(CallAiChatServiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang gọi dịch vụ AI Chat với SessionId: {SessionId}", request.ChatRequest.SessionId);

        // Gọi dịch vụ AI Chat bên ngoài
        var chatResponseResult = await _aiChatService.CallChatWebhookAsync(request.ChatRequest, cancellationToken);

        if (!chatResponseResult.IsSuccess)
        {
            _logger.LogError("Gọi dịch vụ AI Chat thất bại: {Error}", chatResponseResult.Error);
            return Result<ChatResponse>.Failure(chatResponseResult.Error ?? "Không thể nhận phản hồi từ dịch vụ AI Chat.", chatResponseResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        _logger.LogInformation("Đã nhận phản hồi từ dịch vụ AI Chat.");
        return Result<ChatResponse>.Success(chatResponseResult.Value!); // Value is guaranteed not null if IsSuccess is true
    }
}
