using System.Text.Json;
using backend.Application.AI.Prompts;
using backend.Application.Common.Constants;
using backend.Application.Common.Interfaces;
using backend.Application.Common.Models;
using backend.Application.Memories.DTOs; // PhotoAnalysisResultDto, GenerateStoryResponseDto

namespace backend.Application.Memories.Commands.GenerateStory;

public class GenerateStoryCommandHandler : IRequestHandler<GenerateStoryCommand, Result<GenerateStoryResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly IN8nService _n8nService; // NEW

    public GenerateStoryCommandHandler(IApplicationDbContext context, IAuthorizationService authorizationService, IN8nService n8nService)
    {
        _context = context;
        _authorizationService = authorizationService;
        _n8nService = n8nService;
    }

    public async Task<Result<GenerateStoryResponseDto>> Handle(GenerateStoryCommand request, CancellationToken cancellationToken)
    {
        // Find the member to ensure it exists and belongs to the family
        var member = await _context.Members
            .Include(m => m.Family)
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member == null)
        {
            return Result<GenerateStoryResponseDto>.Failure(string.Format(ErrorMessages.NotFound, $"Member with ID {request.MemberId} not found."), ErrorSources.NotFound);
        }

        // Authorization check (can access family of the member)
        if (!_authorizationService.CanAccessFamily(member.FamilyId))
        {
            return Result<GenerateStoryResponseDto>.Failure(ErrorMessages.AccessDenied, ErrorSources.Forbidden);
        }

        // Build the user message for the AI Agent
        var sessionId = Guid.NewGuid().ToString(); // Generate a new session ID for this generation
        var userMessage = PromptBuilder.BuildStoryGenerationPrompt(request, member, member.Family, request.PhotoAnalysisResult);

        // Call n8n Chat Webhook for story generation
        var n8nChatResult = await _n8nService.CallChatWebhookAsync(sessionId, userMessage, cancellationToken);

        if (!n8nChatResult.IsSuccess)
        {
            return Result<GenerateStoryResponseDto>.Failure(n8nChatResult.Error ?? "Lỗi khi gọi webhook n8n để tạo câu chuyện.", ErrorSources.ExternalServiceError);
        }

        GenerateStoryResponseDto? storyResponseDto;
        try
        {
            if (string.IsNullOrEmpty(n8nChatResult.Value))
            {
                return Result<GenerateStoryResponseDto>.Failure("Tạo câu chuyện từ n8n trả về phản hồi rỗng.", ErrorSources.ExternalServiceError);
            }
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            storyResponseDto = System.Text.Json.JsonSerializer.Deserialize<GenerateStoryResponseDto>(n8nChatResult.Value, options);
        }
        catch (JsonException ex)
        {
            return Result<GenerateStoryResponseDto>.Failure($"Tạo câu chuyện từ n8n trả về phản hồi không hợp lệ: {ex.Message}", ErrorSources.ExternalServiceError);
        }

        if (storyResponseDto == null)
        {
            return Result<GenerateStoryResponseDto>.Failure("Tạo câu chuyện từ n8n trả về phản hồi rỗng hoặc không hợp lệ.");
        }

        return Result<GenerateStoryResponseDto>.Success(storyResponseDto);
    }
}
