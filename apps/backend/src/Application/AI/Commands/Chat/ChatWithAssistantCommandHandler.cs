using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Chat;

/// <summary>
/// Handler cho lệnh <see cref="ChatWithAssistantCommand"/>.
/// </summary>
public class ChatWithAssistantCommandHandler : IRequestHandler<ChatWithAssistantCommand, Result<ChatResponse>>
{
    private readonly IAiChatService _chatAiService;
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ChatWithAssistantCommandHandler> _logger;

    public ChatWithAssistantCommandHandler(IAiChatService chatAiService, IApplicationDbContext context, ICurrentUser currentUser, ILogger<ChatWithAssistantCommandHandler> logger)
    {
        _chatAiService = chatAiService;
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<ChatResponse>> Handle(ChatWithAssistantCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra xác thực người dùng và quyền truy cập
        if (!_currentUser.IsAuthenticated)
        {
            return Result<ChatResponse>.Failure(ErrorMessages.Unauthorized, ErrorSources.Authentication);
        }

        var family = await _context.Families
            .Include(f => f.FamilyLimitConfiguration)
            .FirstOrDefaultAsync(f => f.Id == request.FamilyId, cancellationToken);

        if (family == null)
        {
            return Result<ChatResponse>.NotFound(string.Format(ErrorMessages.FamilyNotFound, request.FamilyId), ErrorSources.NotFound);
        }

        // Đảm bảo FamilyLimitConfiguration được khởi tạo
        var newConfigCreated = family.EnsureFamilyLimitConfigurationExists();
        if (newConfigCreated)
        {
            _context.FamilyLimitConfigurations.Add(family.FamilyLimitConfiguration!); // FamilyLimitConfiguration will not be null here
        }
        await _context.SaveChangesAsync(cancellationToken); // Lưu để đảm bảo FamilyLimitConfiguration được thêm vào nếu mới tạo

        // 2. Kiểm tra hạn ngạch AI Chat
        if (family.FamilyLimitConfiguration!.AiChatMonthlyUsage >= family.FamilyLimitConfiguration.AiChatMonthlyLimit)
        {
            _logger.LogWarning("AI Chat quota exceeded for FamilyId {FamilyId}. Usage: {Usage}, Limit: {Limit}",
                request.FamilyId, family.FamilyLimitConfiguration.AiChatMonthlyUsage, family.FamilyLimitConfiguration.AiChatMonthlyLimit);
            return Result<ChatResponse>.Failure(ErrorMessages.AiChatQuotaExceeded, ErrorSources.QuotaExceeded);
        }

        // 3. Ghi nhận sử dụng và lưu thay đổi
        family.FamilyLimitConfiguration.IncrementAiChatUsage();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AI Chat usage incremented for FamilyId {FamilyId}. New usage: {Usage}",
            request.FamilyId, family.FamilyLimitConfiguration.AiChatMonthlyUsage);

        // 4. Gọi dịch vụ AI Chat
        var chatRequest = new ChatRequest
        {
            SessionId = request.SessionId,
            ChatInput = request.ChatInput,
            Metadata = request.Metadata ?? new Dictionary<string, object>()
        };
        chatRequest.Metadata[MetadataConstants.FamilyId] = request.FamilyId.ToString();
        return await _chatAiService.CallChatWebhookAsync(chatRequest, cancellationToken);
    }
}
