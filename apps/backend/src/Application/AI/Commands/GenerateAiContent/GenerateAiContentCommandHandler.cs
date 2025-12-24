using System.Dynamic;
using System.Text.Json; // For JsonSerializerOptions and deserialization
using backend.Application.AI.DTOs;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Events.Queries; // For EventDto
using backend.Application.Families.Commands.IncrementFamilyAiChatUsage;
using backend.Application.Families.Queries; // For FamilyDto
using backend.Application.Members.Queries; // For MemberDto
using backend.Application.Prompts.Queries.GetPromptById;
using Microsoft.Extensions.Logging;

namespace backend.Application.AI.Commands.GenerateAiContent;

/// <summary>
/// Handler cho lệnh GenerateAiContentCommand.
/// </summary>
public class GenerateAiContentCommandHandler : IRequestHandler<GenerateAiContentCommand, Result<List<ExpandoObject>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly IAiGenerateService _aiGenerateService;
    private readonly ILogger<GenerateAiContentCommandHandler> _logger;

    public GenerateAiContentCommandHandler(IApplicationDbContext context, IMediator mediator, IAiGenerateService aiGenerateService, ILogger<GenerateAiContentCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _aiGenerateService = aiGenerateService;
        _logger = logger;
    }

    public async Task<Result<List<ExpandoObject>>> Handle(GenerateAiContentCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra và tăng hạn mức sử dụng AI
        var incrementUsageResult = await _mediator.Send(new IncrementFamilyAiChatUsageCommand { FamilyId = request.FamilyId }, cancellationToken);
        if (!incrementUsageResult.IsSuccess)
        {
            _logger.LogWarning("AI content generation failed for family {FamilyId} due to quota: {Error}", request.FamilyId, incrementUsageResult.Error);
            return Result<List<ExpandoObject>>.Failure(incrementUsageResult.Error ?? ErrorMessages.AiChatQuotaExceeded, incrementUsageResult.ErrorSource ?? ErrorSources.QuotaExceeded);
        }

        // 2. Lấy System Prompt thống nhất
        string promptCode = PromptConstants.UnifiedAiContentGenerationPromptCode;
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
            return Result<List<ExpandoObject>>.Failure(errorMessage, ErrorSources.InvalidConfiguration);
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
            return Result<List<ExpandoObject>>.Failure(combinedResult.Error ?? "Lỗi không xác định khi tạo nội dung AI tổng hợp.", combinedResult.ErrorSource ?? ErrorSources.ExternalServiceError);
        }

        if (combinedResult.Value == null)
        {
            return Result<List<ExpandoObject>>.Failure(ErrorMessages.NoAIResponse, ErrorSources.ExternalServiceError);
        }

        var cards = new List<ExpandoObject>();
        int idCounter = 1;

        // Xử lý Families
        if (combinedResult.Value.Families != null)
        {
            foreach (var family in combinedResult.Value.Families)
            {
                dynamic card = new ExpandoObject();
                card.id = (idCounter++).ToString();
                card.type = "Family";
                card.title = family.Name;
                card.summary = family.Description; // Or other suitable summary
                cards.Add(card);
            }
        }

        // Xử lý Members
        if (combinedResult.Value.Members != null)
        {
            foreach (var member in combinedResult.Value.Members)
            {
                dynamic card = new ExpandoObject();
                card.id = (idCounter++).ToString();
                card.type = "Member";
                card.title = member.FullName;
                card.summary = $"{member.DateOfBirth?.Year} - {member.DateOfDeath?.Year}"; // Example summary
                cards.Add(card);
            }
        }

        // Xử lý Events
        if (combinedResult.Value.Events != null)
        {
            foreach (var @event in combinedResult.Value.Events)
            {
                dynamic card = new ExpandoObject();
                card.id = (idCounter++).ToString();
                card.type = "Event";
                card.title = @event.Name;
                card.summary = $"{@event.StartDate?.ToShortDateString()} - {@event.Location}"; // Example summary
                cards.Add(card);
            }
        }

        return Result<List<ExpandoObject>>.Success(cards);
    }

}
